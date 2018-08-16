using System;
using System.Diagnostics;

namespace Ranitas.Core
{
    public struct Entity : IEquatable<Entity>
    {
        const int kIDBits = 20;
        const uint kIDMask = (1u << kIDBits) - 1;
        const uint kVersionMask = ~kIDMask;

        public readonly uint mValue;

        internal uint Index { get { return mValue & kIDMask; } }
        internal uint Version { get { return mValue & kVersionMask; } }

        internal Entity(uint index, uint version)
        {
            mValue = (index & kIDMask) | (version << kIDBits);
        }

        public static bool operator ==(Entity a, Entity b) => a.mValue == b.mValue;

        public static bool operator !=(Entity a, Entity b) => a.mValue != b.mValue;

        public bool Equals(Entity other) => mValue == other.mValue;

        public override bool Equals(object obj) => (obj is Entity) && Equals((Entity)obj);

        public override int GetHashCode() => (int)mValue;

        public override string ToString()
        {
            return string.Format("Index: {0}, Version: {1}", Index, Version);
        }

        [Obsolete("Entities are not null")]
        public bool Equals(Entity? other) => false;

        [Obsolete("Entities are not null")]
        public static bool operator ==(Entity? a, Entity b) => false;

        [Obsolete("Entities are not null")]
        public static bool operator !=(Entity? a, Entity b) => true;

        [Obsolete("Entities are not null")]
        public static bool operator ==(Entity a, Entity? b) => false;

        [Obsolete("Entities are not null")]
        public static bool operator !=(Entity a, Entity? b) => true;
    }

    public class EntityRegistry
    {
        public static readonly Entity NullEntity = new Entity(0, 0);

        public EntityRegistry(int maxEntities)
        {
            mEntities = new Entity[maxEntities + 1];
            mEntities[0] = new Entity(0, 1); //Index 0 is never assigned, and NullEntity is invalid.
            mNext = 1;
            for (uint i = 1; i <= maxEntities; ++i)
            {
                mEntities[i] = new Entity(i + 1, 0);    //Point to the next entity available
            }
        }

        private uint mNext;
        private Entity[] mEntities;

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
            mEntities[entity.Index] = new Entity(mNext, entity.Version + 1);
            mNext = entity.Index;
        }

        public bool IsValid(Entity entity)
        {
            return mEntities[entity.Index] == entity;
        }
    }
}
