using System.Collections.Generic;
using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    public class FilteredIndexSet : IReadonlyIndexSet
    {
        private readonly IndexFilter mFilter;
        private readonly IndexSet mIndexSet;

        public FilteredIndexSet(int capacity, List<IReadonlyIndexSet> requirements, List<IReadonlyIndexSet> exclusions)
            :this(capacity, requirements.ToArray(), exclusions.ToArray())
        { }

        public FilteredIndexSet(int capacity, IReadonlyIndexSet[] requirements, IReadonlyIndexSet[] exclusions)
        {
            mFilter = new IndexFilter(requirements, exclusions);
            mIndexSet = new IndexSet(capacity);
            //mInjectors = new List<IValueInjector>();    //TODO: This constructor/class is a mess!
        }

        public bool TryInsert(uint indexID)
        {
            Debug.Assert(!Contains(indexID));
            if (mFilter.PassesFilter(indexID))
            {
                mIndexSet.Add(indexID);
                return true;
            }
            return false;
        }

        public bool Contains(uint indexID)
        {
            return mIndexSet.Contains(indexID);
        }

        public bool Remove(uint indexID)
        {
            if (Contains(indexID))
            {
                mIndexSet.Remove(indexID);
                return true;
            }
            return false;
        }
    }

    public delegate void IndexedValueHandler(uint indexID);

    public interface IValueInjector
    {
        void InjectNewValue(uint indexID);
        void InjectExistingValue(uint indexID);
        void RemoveValue(uint indexID);
    }

    public class ValueInjector<TValue> : IValueInjector where TValue : struct
    {
        //TODO: Take IComponentSet and register modification events inside here?
        private IIndexedSet<TValue> mSourceSet;
        private ValueRegistry<TValue> mTargetRegistry;

        public ValueInjector(IIndexedSet<TValue> source, ValueRegistry<TValue> target)
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
