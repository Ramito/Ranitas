using System.Collections.Generic;

namespace Ranitas.Core.ECS
{
    public class IndexFilter
    {
        //ANNOTATION: I like this class's role as a dumb and simple filter, just the initialization registration is something I would consider changing
        private List<IIndexDirectory> mRequireFilters;
        private List<IIndexDirectory> mExcludeFilters;

        public IndexFilter()
        {
            mRequireFilters = new List<IIndexDirectory>();
            mExcludeFilters = new List<IIndexDirectory>();
        }

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
}
