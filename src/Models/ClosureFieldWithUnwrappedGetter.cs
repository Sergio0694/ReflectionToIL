using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ReflectionToIL.Models
{
    public sealed class ClosureFieldWithUnwrappedGetter
    {
        public FieldInfo Info { get; }

        public IReadOnlyList<FieldInfo> Parents { get; }

        public Func<object, object> Getter { get; private set; }

        public ClosureFieldWithUnwrappedGetter(FieldInfo info, IReadOnlyList<FieldInfo> parents)
        {
            Info = info;
            Parents = parents;
        }

        public void BuildDynamicGetter()
        {
            // Create a new dynamic method
            FieldInfo[] hierarchy = Parents.Append(Info).ToArray();
            Type ownerType = hierarchy[0].DeclaringType;
            DynamicMethod method = new DynamicMethod(
                $"Get{Info.Name}",
                typeof(object),             // The return type
                new[] { typeof(object) },   // A single object parameter
                ownerType);                 // The type that will own the new method

            // Load and cast the argument
            ILGenerator il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, ownerType);

            // Unroll the depth traversal until the last parent
            foreach (FieldInfo parent in Parents)
            {
                il.Emit(OpCodes.Ldfld, parent);
            }

            // Get and box the target field
            il.Emit(OpCodes.Ldfld, Info);
            if (Info.FieldType.IsValueType) il.Emit(OpCodes.Box, Info.FieldType);
            il.Emit(OpCodes.Ret);

            // Create the proper delegate type for the method
            Getter = (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }
    }
}

