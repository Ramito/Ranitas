using System.Collections.Generic;

namespace Ranitas.Core.ECS
{
    public class IndexFilter
    {
        //ANNOTATION: I like this class's role as a dumb and simple filter, just the initialization registration is something I would consider changing
        private readonly IReadonlyIndexSet[] mRequireFilters;
        private readonly IReadonlyIndexSet[] mExcludeFilters;

        public IndexFilter(List<IReadonlyIndexSet> requirements, List<IReadonlyIndexSet> exclusions)
            : this(requirements.ToArray(), exclusions.ToArray())
        { }

        public IndexFilter(IReadonlyIndexSet[] requirements, IReadonlyIndexSet[] exclusions)
        {
            mRequireFilters = requirements;
            mExcludeFilters = exclusions;
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
    }
}
