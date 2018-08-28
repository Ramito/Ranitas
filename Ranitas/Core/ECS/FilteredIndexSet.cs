using System.Collections.Generic;
using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    public class FilteredIndexSet : IndexFilter
    {
        private IndexSet mIndexSet;
        private List<IValueInjector> mInjectors;

        public FilteredIndexSet(int capacity)
        {
            mIndexSet = new IndexSet(capacity);
            mInjectors = new List<IValueInjector>();    //TODO: This constructor/class is a mess!
        }

        public void RegisterRequirement<TValue>(EventSystem.EventSystem eventSystem, IndexedSet<TValue> valueSource, ValueRegistry<TValue> valueTarget) where TValue : struct
        {
            RegisterRequirement(valueSource);
            ValueInjector<TValue> injector = new ValueInjector<TValue>(valueSource, valueTarget);

            mInjectors.Add(injector);

            //TODO: Validate no type collisions between requirements and exclusions?

            eventSystem.AddMessageReceiver<ValueAttachedMessage<TValue>>((msg) => { TryInsert(msg.IndexID); });
            eventSystem.AddMessageReceiver<ValueModifiedEvent<TValue>>((msg) => { UpdateValue(msg.IndexID); });
            eventSystem.AddMessageReceiver<ValueRemovedMessage<TValue>>((msg) => { Remove(msg.IndexID); });
        }

        public void RegisterExclusion<TValue>(EventSystem.EventSystem eventSystem, IndexedSet<TValue> exclusionSource) where TValue : struct
        {
            RegisterExclusion(exclusionSource);

            eventSystem.AddMessageReceiver<ValueAttachedMessage<TValue>>((msg) => { Remove(msg.IndexID); });
            eventSystem.AddMessageReceiver<ValueRemovedMessage<TValue>>((msg) => { TryInsert(msg.IndexID); });
        }

        private void TryInsert(uint indexID)
        {
            Debug.Assert(!mIndexSet.Contains(indexID));
            if (PassesFilter(indexID))
            {
                mIndexSet.Add(indexID);
                foreach (IValueInjector injetor in mInjectors)
                {
                    injetor.InjectNewValue(indexID);
                }
            }
        }

        private void UpdateValue(uint indexID)
        {
            //TODO: Consider batching component value updates so the whole packed array can be flushed, instead of firing an event for each!
            if (mIndexSet.Contains(indexID))
            {
                foreach (IValueInjector injetor in mInjectors)
                {
                    injetor.InjectExistingValue(indexID);
                }
            }
        }

        public void Remove(uint indexID)
        {
            if (mIndexSet.Contains(indexID))
            {
                foreach (IValueInjector injetor in mInjectors)
                {
                    injetor.RemoveValue(indexID);
                }
                mIndexSet.Remove(indexID);
            }
        }

        private interface IValueInjector
        {
            void InjectNewValue(uint indexID);
            void InjectExistingValue(uint indexID);
            void RemoveValue(uint indexID);
        }

        private class ValueInjector<TValue> : IValueInjector where TValue : struct
        {
            private IndexedSet<TValue> mSourceSet;
            private ValueRegistry<TValue> mTargetRegistry;

            public ValueInjector(IndexedSet<TValue> source, ValueRegistry<TValue> target)
            {
                mSourceSet = source;
                mTargetRegistry = target;
            }

            public void InjectNewValue(uint indexID)
            {
                TValue value = mSourceSet.GetValue(indexID);
                mTargetRegistry.AddValue(value);
            }

            public void InjectExistingValue(uint indexID)
            {
                TValue value = mSourceSet.GetValue(indexID);
                uint packedIndex = mSourceSet.GetPackedIndex(indexID);
                mTargetRegistry.SetValue(value, packedIndex);
            }

            public void RemoveValue(uint indexID)
            {
                uint packedIndex = mSourceSet.GetPackedIndex(indexID);
                mTargetRegistry.RemoveValue(packedIndex);
            }
        }
    }
}
