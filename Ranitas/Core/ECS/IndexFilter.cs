using System.Collections.Generic;

namespace Ranitas.Core.ECS
{
    public class IndexFilter
    {
        //ANNOTATION: I like this class's role as a dumb and simple filter, just the initialization registration is something I would consider changing
        private List<IReadonlyIndexSet> mRequireFilters;
        private List<IReadonlyIndexSet> mExcludeFilters;

        public IndexFilter()
        {
            mRequireFilters = new List<IReadonlyIndexSet>();
            mExcludeFilters = new List<IReadonlyIndexSet>();
        }

        public bool PassesFilter(uint index)
        {
            foreach (IReadonlyIndexSet indexDirectory in mRequireFilters)
            {
                if (!indexDirectory.Contains(index))
                {
                    return false;
                }
            }
            foreach (IReadonlyIndexSet indexDirectory in mExcludeFilters)
            {
                if (indexDirectory.Contains(index))
                {
                    return false;
                }
            }
            return true;
        }

        public void RegisterRequirement(IReadonlyIndexSet indexDirectory)
        {
            mRequireFilters.Add(indexDirectory);
        }

        public void RegisterExclusion(IReadonlyIndexSet indexDirectory)
        {
            mExcludeFilters.Add(indexDirectory);
        }
    }
}
