using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    public class SliceRequirementOutput<TComponent> where TComponent : struct
    {
        private RestrictedArray<TComponent> mArray = null;

        public uint Count
        {
            get
            {
                Debug.Assert(mArray != null, "This output has not been linked to an entity slice.");
                return mArray.Count;
            }
        }

        public TComponent this[uint index]
        {
            get
            {
                Debug.Assert(mArray != null, "This output has not been linked to an entity slice.");
                return mArray[index];
            }
        }
    }
}
