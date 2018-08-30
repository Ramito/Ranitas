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
}
