using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    public class IndexFilter
    {
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

    public class FilteredIndexSet
    {
        private IndexFilter mFilter;
        private IndexSet mIndexSet;
        private List<IValueInjector> mInjectors;

        public void RegisterRequirement<TValue>(IndexedSet<TValue> valueSource, ValueRegistry<TValue> valueTarget) where TValue : struct
        {
            mFilter.RegisterRequirement(valueSource);
            ValueInjector<TValue> injector = new ValueInjector<TValue>(valueSource, valueTarget);
            //TODO: Register to add/remove signals!
            mInjectors.Add(injector);
        }

        public void RegisterExclusion<TValue>(IndexedSet<TValue> exclusionSource) where TValue : struct
        {
            mFilter.RegisterExclusion(exclusionSource);
            //TODO: Register to add remove signals!
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
