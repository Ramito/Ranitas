using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    public class IndexFilter
    {
        //ANNOTATION: I like this class's role as a dumb and simple filter, just the initialization registration is something I would consider changing
        private List<IIndexDirectory> mRequireFilters;
        private List<IIndexDirectory> mExcludeFilters;

        public bool PassesFilter(uint index)
        {
            foreach (IIndexDirectory indexDirectory in mRequireFilters)
            {
                if (!indexDirectory.Contains(index))
                {
                    return false;
                }
            }
            foreach (IIndexDirectory indexDirectory in mExcludeFilters)
            {
                if (indexDirectory.Contains(index))
                {
                    return false;
                }
            }
            return true;
        }

        public void RegisterRequirement(IIndexDirectory indexDirectory)
        {
            mRequireFilters.Add(indexDirectory);
        }

        public void RegisterExclusion(IIndexDirectory indexDirectory)
        {
            mExcludeFilters.Add(indexDirectory);
        }
    }

    public class ValueRegistry<TValue> where TValue : struct
    {
        private uint mCount;
        private TValue[] mValues;

        public ValueRegistry(int capacity)
        {
            mCount = 0;
            mValues = new TValue[capacity];
        }

        public void AddValue(TValue value)
        {
            mValues[mCount] = value;
            ++mCount;
        }

        //BUG ERROR WTF This index needs to be looked up from the sparse sets. I am not using it correctly!
        public void SetValue(TValue value, uint index)
        {
            Debug.Assert(mCount > index);
            mValues[index] = value;
        }

        public void RemoveValue(uint index)
        {
            Debug.Assert(mCount > 0);
            --mCount;
            mValues[index] = mValues[mCount];
        }
    }

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

    public delegate void ComponentRemoved<TComponent>(uint index);

    public class FilteredIndexSet
    {
        private IndexFilter mFilter;
        private IndexSet mIndexSet;
        private List<IValueInjector> mInjectors;

        public void RegisterRequirement<TValue>(EventSystem.EventSystem eventSystem, IndexedSet<TValue> valueSource, ValueRegistry<TValue> valueTarget) where TValue : struct
        {
            mFilter.RegisterRequirement(valueSource);
            ValueInjector<TValue> injector = new ValueInjector<TValue>(valueSource, valueTarget);

            mInjectors.Add(injector);

            //TODO: Validate no type collisions between requirements and exclusions?

            eventSystem.AddMessageReceiver<ValueAttachedMessage<TValue>>((msg) => { TryInsert(msg.IndexID); });
            eventSystem.AddMessageReceiver<ValueModifiedEvent<TValue>>((msg) => { UpdateValue(msg.IndexID); });
            eventSystem.AddMessageReceiver<ValueRemovedMessage<TValue>>((msg) => { Remove(msg.IndexID); });
        }

        public void RegisterExclusion<TValue>(EventSystem.EventSystem eventSystem, IndexedSet<TValue> exclusionSource) where TValue : struct
        {
            mFilter.RegisterExclusion(exclusionSource);

            eventSystem.AddMessageReceiver<ValueAttachedMessage<TValue>>((msg) => { Remove(msg.IndexID); });
            eventSystem.AddMessageReceiver<ValueRemovedMessage<TValue>>((msg) => { TryInsert(msg.IndexID); });
        }

        private void TryInsert(uint index)
        {
            if (mFilter.PassesFilter(index))
            {
                mIndexSet.Add(index);
                foreach (IValueInjector injetor in mInjectors)
                {
                    injetor.InjectNewValue(index);
                }
            }
        }

        private void UpdateValue(uint index)
        {
            //TODO: Consider batching component value updates so the whole packed array can be flushed, instead of firing an event for each!
            throw new NotImplementedException();
        }

        private void Remove(uint index)
        {
            if (mIndexSet.Contains(index))
            {
                // TODO: mIndexSet.GetPackedIndex(index);
                mIndexSet.Remove(index);//BUG NO WTF
                foreach (IValueInjector injetor in mInjectors)
                {
                    injetor.RemoveValue(index);
                }
            }
        }

        private interface IValueInjector
        {
            void InjectNewValue(uint index);
            void InjectExistingValue(uint index);
            void RemoveValue(uint index);
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

            public void InjectNewValue(uint index)
            {
                TValue value = mSourceSet.GetValue(index);
                mTargetRegistry.AddValue(value);
            }

            public void InjectExistingValue(uint index)
            {
                TValue value = mSourceSet.GetValue(index);
                mTargetRegistry.SetValue(value, index);
            }

            public void RemoveValue(uint index)
            {
                mTargetRegistry.RemoveValue(index);
            }
        }
    }
}
