using System;
using System.Reflection;
using static Ranitas.Core.ECS.EntityRegistry;

namespace Ranitas.Core.ECS
{
    public static class EntityRegistryExtensions
    {
        public static void SetupSlice<TSlice>(this EntityRegistry registry, ref TSlice slice) where TSlice : struct
        {
            EntitySliceConfiguration sliceConfiguration = registry.ConfigureSlice();

            object boxedSlice = slice;  //Box it so we can set members with reflection (otherwise they are lost when passed by value)
            Type sliceType = typeof(TSlice);
            //Find slice output and config members:
            FieldInfo[] memberFields = sliceType.GetFields();
            foreach (FieldInfo field in memberFields)
            {
                Type fieldType = field.FieldType;
                if (fieldType.Name == typeof(SliceRequirementOutput<>).Name)
                {
                    //Instantiate output and set it back on the slice struct
                    object outputInstance = Activator.CreateInstance(fieldType);

                    //Configure entity slice to require this component type and target this output
                    foreach (MethodInfo methodInfo in typeof(EntitySliceConfiguration).GetMethods())
                    {
                        if (methodInfo.IsGenericMethod && (methodInfo.Name == "Require") && (methodInfo.GetParameters().Length == 1))
                        {
                            MethodInfo requireMethod = methodInfo.MakeGenericMethod(fieldType.GenericTypeArguments[0]);
                            sliceConfiguration = (EntitySliceConfiguration)requireMethod.Invoke(sliceConfiguration, new object[] { outputInstance });
                            field.SetValue(boxedSlice, outputInstance);
                            break;
                        }
                    }
                }
                else if (fieldType.Name == typeof(SliceRequirement<>).Name)
                {
                    //Configure entity slice to require this component type
                    foreach (MethodInfo methodInfo in typeof(EntitySliceConfiguration).GetMethods())
                    {
                        if (methodInfo.IsGenericMethod && (methodInfo.Name == "Require") && (methodInfo.GetParameters().Length == 0))
                        {
                            MethodInfo requireMethod = methodInfo.MakeGenericMethod(fieldType.GenericTypeArguments[0]);
                            sliceConfiguration = (EntitySliceConfiguration)requireMethod.Invoke(sliceConfiguration, null);
                            break;
                        }
                    }
                }
                else if (fieldType.Name == typeof(SliceExclusion<>).Name)
                {
                    //Configure entity slice to exclude this component type
                    MethodInfo requireMethod = typeof(EntitySliceConfiguration).GetMethod("Exclude");
                    MethodInfo genericRequire = requireMethod.MakeGenericMethod(fieldType.GenericTypeArguments[0]);
                    sliceConfiguration = (EntitySliceConfiguration)genericRequire.Invoke(sliceConfiguration, null);
                }

            }
            sliceConfiguration.CreateSlice();   //TODO: Would be great to check if a slice is just like another existing slice, and if so just reuse!
            slice = (TSlice)boxedSlice;
        }
    }
}
