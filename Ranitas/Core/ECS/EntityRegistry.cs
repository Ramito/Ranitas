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
            uint entityIndex = entity.Index;
            foreach (IUntypedIndexedSet componentSet in mComponentSets)
            {
                if (componentSet.Contains(entityIndex))
                {
                    componentSet.Remove(entityIndex);
                }
            }
            Debug.Assert(IsValid(entity), "Can't destroy invalid entity.");
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
    }
}
