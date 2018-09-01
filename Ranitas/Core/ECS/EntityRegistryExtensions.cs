using System;
using System.Reflection;
using static Ranitas.Core.ECS.EntityRegistry;

namespace Ranitas.Core.ECS
{
    public static class EntityRegistryExtensions
    {
        //TODO: Maybe this should return the created slice rather than take a ref
        public static void MakeMagicSlice<TSlice>(this EntityRegistry registry, ref TSlice slice)
        {
            object boxedSlice = slice;
            Type sliceType = typeof(TSlice);

            EntitySliceConfiguration sliceConfiguration = registry.ConfigureSlice();

            FieldInfo[] memberFields = sliceType.GetFields();
            foreach (FieldInfo field in memberFields)
            {
                Type fieldType = field.FieldType;
                if (fieldType.Name == typeof(ValueRegistry<>).Name)
                {
                    //Instantiate registry and set it back on the slice
                    object registryInstance = Activator.CreateInstance(fieldType, new object[] { registry.Capacity });
                    field.SetValue(boxedSlice, registryInstance);

                    //Configure entity slice to require this component type and target this registry
                    MethodInfo requireMethod = typeof(EntitySliceConfiguration).GetMethod("Require");
                    MethodInfo genericRequire = requireMethod.MakeGenericMethod(fieldType.GenericTypeArguments[0]);
                    sliceConfiguration = (EntitySliceConfiguration)genericRequire.Invoke(sliceConfiguration, new object[] { registryInstance });
                }

            }
            sliceConfiguration.CreateSlice();   //TODO: Would be great to check if a slice is just like another existing slice, and if so just reuse!
            slice = (TSlice)boxedSlice;
        }
    }
}
