using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ReflectionToIL.Models
{
    public sealed class ClosureFieldWithGetter
    {
        public FieldInfo Info { get; }

        public IReadOnlyList<ClosureFieldWithGetter> Parents { get; }

        public Func<object, object> Getter { get; private set; }

        public ClosureFieldWithGetter(FieldInfo info, IReadOnlyList<ClosureFieldWithGetter> parents)
        {
            Info = info;
            Parents = parents;
        }

        public void BuildDynamicGetter()
        {
            // Create a new dynamic method
            Type ownerType = Info.DeclaringType;
            DynamicMethod method = new DynamicMethod(
                $"Get{Info.Name}",
                typeof(object),             // The return type
                new[] { typeof(object) },  // A single object parameter
                ownerType);                 // The type that will own the new method

            // Build the IL method
            ILGenerator il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, ownerType);
            il.Emit(OpCodes.Ldfld, Info);
            if (Info.FieldType.IsValueType) il.Emit(OpCodes.Box, Info.FieldType);
            il.Emit(OpCodes.Ret);

            // Create the proper delegate type for the method
            Getter = (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }
    }
}
