using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Ranitas.Core.ECS
{
    public sealed class EntityRegistry : IReadonlyIndexedSet<Entity>
    {
        public EntityRegistry(int maxEntities)
        {
            Debug.Assert(maxEntities <= Entity.MaxIndex, "Can't store this many entities!");
            mEntities = new Entity[maxEntities + 1];
            mEntities[0] = new Entity(0, int.MaxValue); //Index 0 is never assigned, and NullEntity is invalid.
            mNext = 1;
            for (int i = 1; i <= maxEntities; ++i)
            {
                mEntities[i] = new Entity(i + 1, 0);    //Point to the next entity available
            }
        }

        public int Capacity
        {
            get { return mEntities.Length - 1; }
        }

        public Entity Create()
        {
            int createdIndex = mNext;
            Debug.Assert(createdIndex < mEntities.Length, "Entities have run out!");
            Entity inPlaceEntity = mEntities[createdIndex];
            mNext = inPlaceEntity.Index;
            Entity createdEntity = new Entity(createdIndex, inPlaceEntity.Version);
            mEntities[createdIndex] = createdEntity;
            return createdEntity;
        }

        public void Destroy(Entity entity)
        {
            Debug.Assert(IsValid(entity), "Can't destroy invalid entity.");
            int entityIndex = entity.Index;

            //It is important to clear slices before touching the component sets, as the component sets drive the slices internally!
            foreach (IUntypedComponentSet componentSet in mComponentSets)
            {
                if (componentSet.Contains(entityIndex))
                {
                    componentSet.Remove(entityIndex);
                }
            }
            //Remove from any slice made only of exclusions!
            foreach (EntitySlice slice in mExludeOnlySlice)
            {
                slice.RemoveValue(entityIndex);
            }

            mEntities[entityIndex] = new Entity(mNext, entity.Version + 1);
            mNext = entityIndex;
        }

        public bool IsValid(Entity entity)
        {
            return mEntities[entity.Index] == entity;
        }

        public bool HasComponent<TComponent>(Entity entity) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            IUntypedComponentSet componentSet = GetUntypedIndexedSet<TComponent>();
            return componentSet.Contains(entity.Index);
        }

        public void AddComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            ComponentSet<TComponent> componentSet = GetComponentSet<TComponent>();
            componentSet.Add(component, entity.Index);
        }

        public void SetComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            ComponentSet<TComponent> componentSet = GetComponentSet<TComponent>();
            componentSet.Replace(component, entity.Index);
        }

        public void SetOrAddComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            ComponentSet<TComponent> componentSet = GetComponentSet<TComponent>();
            componentSet.AddOrReplace(component, entity.Index);
        }

        public TComponent GetComponent<TComponent>(Entity entity) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            ComponentSet<TComponent> componentSet = GetComponentSet<TComponent>();
            return componentSet.GetValue(entity.Index);
        }

        public void RemoveComponent<TComponent>(Entity entity) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            IUntypedComponentSet componentSet = GetUntypedIndexedSet<TComponent>();
            componentSet.Remove(entity.Index);
        }

        public EntitySliceConfiguration BeginSlice()
        {
            EntitySliceConfiguration newSlice = new EntitySliceConfiguration(this);
            return newSlice;
        }

        private void ValidateOrRegisterComponentType<TComponent>() where TComponent : struct
        {
            Type componentType = typeof(TComponent);
            if (!mComponentSetLookup.ContainsKey(componentType))
            {
                ComponentSet<TComponent> componentSet = new ComponentSet<TComponent>(mEntities.Length);
                ushort lookup = (ushort)mComponentSets.Count;
                mComponentSets.Add(componentSet);
                mComponentSetLookup.Add(componentType, lookup);
            }
        }

        private IUntypedComponentSet GetUntypedIndexedSet<TComponent>() where TComponent : struct
        {
            Type componentType = typeof(TComponent);
            ValidateOrRegisterComponentType<TComponent>();
            ushort lookup = mComponentSetLookup[componentType];
            return mComponentSets[lookup];
        }

        private ComponentSet<TComponent> GetComponentSet<TComponent>() where TComponent : struct
        {
            Type componentType = typeof(TComponent);
            ValidateOrRegisterComponentType<TComponent>();
            ushort lookup = mComponentSetLookup[componentType];
            return (ComponentSet<TComponent>)mComponentSets[lookup];
        }

        Entity IReadonlyIndexedSet<Entity>.GetValue(int indexID)
        {
            return mEntities[indexID];
        }

        private int mNext;
        private Entity[] mEntities;

        private List<IUntypedComponentSet> mComponentSets = new List<IUntypedComponentSet>();
        private Dictionary<Type, ushort> mComponentSetLookup = new Dictionary<Type, ushort>();

        private List<EntitySlice> mRegisteredSlices = new List<EntitySlice>();  //TODO: This is mostly useful for cleanup purposes. But we do not do any cleanup yet!!!
        private List<EntitySlice> mExludeOnlySlice = new List<EntitySlice>();

        #region Entity slice classes and interfaces
        public class EntitySliceConfiguration
        {
            private EntityRegistry mRegistry;
            List<Tuple<IPublishingIndexSet, ValueInjectorBase>> mMatchedInjectorRequirements = new List<Tuple<IPublishingIndexSet, ValueInjectorBase>>();
            List<IPublishingIndexSet> mLooseRequirements = new List<IPublishingIndexSet>();
            List<IPublishingIndexSet> mExclusions = new List<IPublishingIndexSet>();
            List<ValueInjectorBase> mLooseInjectors = new List<ValueInjectorBase>();

            public EntitySliceConfiguration(EntityRegistry registry)
            {
                mRegistry = registry;
            }

            public EntitySliceConfiguration Require<TComponent>(SliceRequirementOutput<TComponent> targetOutput) where TComponent : struct
            {
                Debug.Assert(targetOutput != null, "Target output cannot be null before calling this method!");

                ComponentSet<TComponent> componentSet = mRegistry.GetComponentSet<TComponent>();
                RestrictedArray<TComponent> writeArray = new RestrictedArray<TComponent>(mRegistry.Capacity);
                ValueInjector<TComponent> injector = new ValueInjector<TComponent>(componentSet, writeArray);
                componentSet.ValueModified += injector.InjectExistingValue;

                Tuple<IPublishingIndexSet, ValueInjectorBase> matchPair = new Tuple<IPublishingIndexSet, ValueInjectorBase>(componentSet, injector);
                mMatchedInjectorRequirements.Add(matchPair);

                //Set the write array to be accessed by the component output passed
                Type outputType = typeof(SliceRequirementOutput<TComponent>);
                FieldInfo arrayField = outputType.GetField("mArray", BindingFlags.NonPublic | BindingFlags.Instance);
                arrayField.SetValue(targetOutput, writeArray);

                return this;
            }

            public EntitySliceConfiguration GetEntities(SliceEntityOutput output)
            {
                RestrictedArray<Entity> writeArray = new RestrictedArray<Entity>(mRegistry.Capacity);
                ValueInjector<Entity> injector = new ValueInjector<Entity>(mRegistry, writeArray);
                mLooseInjectors.Add(injector);

                //InjectExistingValue is impossible to use in this context. An entity would never be replaced, without being destroyed and then created

                //Set the write array to be accessed by the component output passed
                Type outputType = typeof(SliceEntityOutput);
                FieldInfo arrayField = outputType.GetField("mArray", BindingFlags.NonPublic | BindingFlags.Instance);
                arrayField.SetValue(output, writeArray);

                return this;
            }

            public EntitySliceConfiguration Require<TComponent>() where TComponent : struct
            {
                ComponentSet<TComponent> componentSet = mRegistry.GetComponentSet<TComponent>();
                mLooseRequirements.Add(componentSet);
                return this;
            }

            public EntitySliceConfiguration Exclude<TComponent>() where TComponent : struct
            {
                ComponentSet<TComponent> componentSet = mRegistry.GetComponentSet<TComponent>();
                mExclusions.Add(componentSet);
                return this;
            }

            public void CompleteSlice()
            {
                //WARNING: The following constructor WILL modify the passed collections!
                EntitySlice slice = new EntitySlice(mRegistry.mEntities.Length, mMatchedInjectorRequirements, mLooseRequirements, mExclusions, mLooseInjectors);
                mRegistry.mRegisteredSlices.Add(slice);
                if ((mMatchedInjectorRequirements.Count + mLooseRequirements.Count) == 0)
                {
                    mRegistry.mExludeOnlySlice.Add(slice);
                }
                //Invalidate further use
                mRegistry = null;
                mMatchedInjectorRequirements.Clear();
                mLooseRequirements.Clear();
                mExclusions.Clear();
                mLooseInjectors.Clear();
            }
        }

        private class EntitySlice
        {
            private readonly FilteredIndexSet mFilteredSet;
            private readonly ValueInjectorBase[] mInjectors;

            public EntitySlice(int capacity, List<Tuple<IPublishingIndexSet, ValueInjectorBase>> matchedInjectorRequirements, List<IPublishingIndexSet> looseRequirements, List<IPublishingIndexSet> exclusions, List<ValueInjectorBase> looseInjectors)
            {
                for (int i = 0; i < matchedInjectorRequirements.Count; ++i)
                {
                    Tuple<IPublishingIndexSet, ValueInjectorBase> tuple = matchedInjectorRequirements[i];
                    IPublishingIndexSet publisher = tuple.Item1;
                    ValueInjectorBase matchedInjector = tuple.Item2;
                    //The injector needs to be told the filter, this is done later, once the filter is created!

                    looseRequirements.Add(publisher);
                    looseInjectors.Add(matchedInjector);
                }

                foreach (IPublishingIndexSet requirement in looseRequirements)
                {
                    requirement.PostNewValue += TryAddValue;
                    requirement.PreRemoved += RemoveValue;
                }

                foreach (IPublishingIndexSet exclusion in exclusions)
                {
                    exclusion.PreNewValue += RemoveValue;
                    exclusion.PostRemoved += TryAddValue;
                }

                IReadonlyIndexSet[] reqArray = ArrayCastedType(looseRequirements);
                IReadonlyIndexSet[] exclArray = ArrayCastedType(exclusions);
                mFilteredSet = new FilteredIndexSet(capacity, reqArray, exclArray);

                for (int i = 0; i < matchedInjectorRequirements.Count; ++i)
                {
                    Tuple<IPublishingIndexSet, ValueInjectorBase> tuple = matchedInjectorRequirements[i];
                    ValueInjectorBase matchedInjector = tuple.Item2;
                    matchedInjector.IndexFilter = mFilteredSet;
                }

                mInjectors = looseInjectors.ToArray();
            }

            private static IReadonlyIndexSet[] ArrayCastedType(List<IPublishingIndexSet> items)
            {
                IReadonlyIndexSet[] itemsArray = new IReadonlyIndexSet[items.Count];
                for (int i = 0; i < items.Count; ++i)
                {
                    itemsArray[i] = items[i];
                }
                return itemsArray;
            }

            private void TryAddValue(int indexID)
            {
                if (mFilteredSet.TryInsert(indexID))
                {
                    foreach (ValueInjectorBase injector in mInjectors)
                    {
                        injector.InjectNewValue(indexID);
                    }
                }
            }

            public void RemoveValue(int indexID)
            {
                if (mFilteredSet.Contains(indexID))
                {
                    int packedIndex = mFilteredSet.GetPackedIndex(indexID);
                    mFilteredSet.Remove(indexID);
                    foreach (ValueInjectorBase injector in mInjectors)
                    {
                        injector.RemoveValue(indexID, packedIndex);
                    }
                }
            }
        }

        private abstract class ValueInjectorBase
        {
            public IReadonlyIndexSet IndexFilter;
            public abstract void InjectNewValue(int indexID);
            public abstract void RemoveValue(int indexID, int packedIndex);
        }

        private class ValueInjector<TValue> : ValueInjectorBase where TValue : struct
        {
            private IReadonlyIndexedSet<TValue> mSourceSet;
            private RestrictedArray<TValue> mTargetOutput;

            public ValueInjector(IReadonlyIndexedSet<TValue> source, RestrictedArray<TValue> target)
            {
                mSourceSet = source;
                mTargetOutput = target;
            }

            public override void InjectNewValue(int indexID)
            {
                TValue value = mSourceSet.GetValue(indexID);
                mTargetOutput.AddValue(value);
            }

            public void InjectExistingValue(int indexID, TValue value)
            {
                if (IndexFilter.Contains(indexID))
                {
                    int packedIndex = IndexFilter.GetPackedIndex(indexID);
                    mTargetOutput.SetValue(value, packedIndex);
                }
            }

            public override void RemoveValue(int indexID, int packedIndex)
            {
                mTargetOutput.RemoveValue(packedIndex);
            }
        }
        #endregion
    }
}
