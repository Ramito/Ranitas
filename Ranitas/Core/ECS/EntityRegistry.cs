using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    public class EntityRegistry
    {
        public EntityRegistry(int maxEntities)
        {
            Debug.Assert(maxEntities <= Entity.MaxIndex, "Can't store this many entities!");
            mEntities = new Entity[maxEntities + 1];
            mEntities[0] = new Entity(0, uint.MaxValue); //Index 0 is never assigned, and NullEntity is invalid.
            mNext = 1;
            for (uint i = 1; i <= maxEntities; ++i)
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
            uint createdIndex = mNext;
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
            uint entityIndex = entity.Index;

            //Remove from any slices! This is not done through remove value/component events since a slice can be made up of only exclusions
            foreach (EntitySlice slice in mRegisteredSlices)
            {
                slice.Remove(entityIndex);
            }
            //It is important to clear slices before touching the component sets, as the component sets drive the slices internally!
            foreach (IUntypedIndexedSet componentSet in mComponentSets)
            {
                if (componentSet.Contains(entityIndex))
                {
                    componentSet.Remove(entityIndex);
                }
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
            IUntypedIndexedSet componentSet = GetUntypedIndexedSet<TComponent>();
            return componentSet.Contains(entity.Index);
        }

        public void AddComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            IndexedSet<TComponent> componentSet = GetIndexedSet<TComponent>();
            componentSet.Add(component, entity.Index);

            mEventSystem.PostMessage(new ValueAttachedMessage<TComponent>(entity.Index));
        }

        public void SetComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            IndexedSet<TComponent> componentSet = GetIndexedSet<TComponent>();
            componentSet.Replace(component, entity.Index);

            mEventSystem.PostMessage(new ValueModifiedEvent<TComponent>(entity.Index));
            //TODO: Do I need a way to update these en masse, rather than posting for each?
        }

        public void SetOrAddComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            IndexedSet<TComponent> componentSet = GetIndexedSet<TComponent>();
            componentSet.AddOrReplace(component, entity.Index);
        }

        public TComponent GetComponent<TComponent>(Entity entity) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            IndexedSet<TComponent> componentSet = GetIndexedSet<TComponent>();
            return componentSet.GetValue(entity.Index);
        }

        public void RemoveComponent<TComponent>(Entity entity) where TComponent : struct
        {
            Debug.Assert(IsValid(entity));
            IUntypedIndexedSet componentSet = GetUntypedIndexedSet<TComponent>();
            componentSet.Remove(entity.Index);

            mEventSystem.PostMessage(new ValueRemovedMessage<TComponent>(entity.Index));
        }

        public EntitySlice BeginSlice()
        {
            EntitySlice newSlice = new EntitySlice(this);
            return newSlice;
        }

        private void ValidateOrRegisterComponentType<TComponent>() where TComponent : struct
        {
            Type componentType = typeof(TComponent);
            if (!mComponentSetLookup.ContainsKey(componentType))
            {
                IndexedSet<TComponent> componentSet = new IndexedSet<TComponent>(mEntities.Length);
                ushort lookup = (ushort)mComponentSets.Count;
                mComponentSets.Add(componentSet);
                mComponentSetLookup.Add(componentType, lookup);
            }
        }

        private IUntypedIndexedSet GetUntypedIndexedSet<TComponent>() where TComponent : struct
        {
            Type componentType = typeof(TComponent);
            ValidateOrRegisterComponentType<TComponent>();
            ushort lookup = mComponentSetLookup[componentType];
            return mComponentSets[lookup];
        }

        private IndexedSet<TComponent> GetIndexedSet<TComponent>() where TComponent : struct
        {
            Type componentType = typeof(TComponent);
            ValidateOrRegisterComponentType<TComponent>();
            ushort lookup = mComponentSetLookup[componentType];
            return (IndexedSet<TComponent>)mComponentSets[lookup];
        }

        private uint mNext;
        private Entity[] mEntities;

        private List<IUntypedIndexedSet> mComponentSets = new List<IUntypedIndexedSet>();
        private Dictionary<Type, ushort> mComponentSetLookup = new Dictionary<Type, ushort>();

        private EventSystem.EventSystem mEventSystem = new EventSystem.EventSystem();

        private List<EntitySlice> mRegisteredSlices = new List<EntitySlice>();
        public class EntitySlice : FilteredIndexSet
        {
            private EntityRegistry mRegistry;

            public EntitySlice(EntityRegistry registry) : base(registry.Capacity + 1)
            {
                mRegistry = registry;
            }

            public EntitySlice Require<TComponent>(ValueRegistry<TComponent> targetRegistry) where TComponent : struct
            {
                IndexedSet<TComponent> componentSet = mRegistry.GetIndexedSet<TComponent>();
                RegisterRequirement(mRegistry.mEventSystem, componentSet, targetRegistry);
                return this;
            }

            public EntitySlice Exclude<TComponent>() where TComponent : struct
            {
                IndexedSet<TComponent> componentSet = mRegistry.GetIndexedSet<TComponent>();
                RegisterExclusion(mRegistry.mEventSystem, componentSet);
                return this;
            }

            public void CloseSlice()
            {
                //TODO: Actually close slice
                //TODO: Register slice events at this time!(?)
                mRegistry.mRegisteredSlices.Add(this);
                mRegistry = null;
            }
        }
    }

    //TODO: Hookup delegates directly not through events?
    public struct ValueAttachedMessage<TComponent> where TComponent : struct
    {
        public ValueAttachedMessage(uint index)
        {
            IndexID = index;
        }

        public readonly uint IndexID;   //TODO: Need to formalize and consistently use index language (packed (id) vs sparse (?))
    }

    public struct ValueRemovedMessage<TComponent> where TComponent : struct
    {
        public ValueRemovedMessage(uint index)
        {
            IndexID = index;
        }

        public readonly uint IndexID;
    }

    public struct ValueModifiedEvent<TComponent> where TComponent : struct
    {
        public ValueModifiedEvent(uint index)
        {
            IndexID = index;
        }

        public readonly uint IndexID;
    }
}
