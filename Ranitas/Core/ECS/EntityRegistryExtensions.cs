using System;
using System.Reflection;

namespace Ranitas.Core.ECS
{
    public static class EntityRegistryExtensions
    {
        //TODO: Maybe this should return the created slice rather than take a ref
        public static void MakeMagicSlice<TSlice>(this EntityRegistry registry, ref TSlice slice)
        {
            object boxedSlice = slice;
            Type sliceType = typeof(TSlice);

            var sliceConfiguration = registry.ConfigureSlice();

            FieldInfo[] memberFields = sliceType.GetFields();
            foreach (FieldInfo field in memberFields)
            {
                Type fieldType = field.FieldType;
                if (fieldType.Name == typeof(ValueRegistry<>).Name)
                {
                    object fieldInstance = Activator.CreateInstance(fieldType, new object[] { registry.Capacity });
                    //Call require on the slice, but then we need to be able to invoke with the right generic parameter!
                    field.SetValue(boxedSlice, fieldInstance);
                }

            }
            slice = (TSlice)boxedSlice;
        }
    }
}
