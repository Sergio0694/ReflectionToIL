using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ReflectionToIL.Models
{
    /// <summary>
    /// A simple model that wraps a <see cref="FieldInfo"/> instance and a list of its parents from a given root
    /// </summary>
    public sealed class ClosureFieldWithUnwrappedGetter
    {
        /// <summary>
        /// The wrapped <see cref="FieldInfo"/> instance
        /// </summary>
        public FieldInfo Info { get; }

        /// <summary>
        /// The list of <see cref="FieldInfo"/> values to traverse the closure class hierarchy and reach the target field
        /// </summary>
        public IReadOnlyList<FieldInfo> Parents { get; }

        /// <summary>
        /// Gets or sets a <see cref="Func{T,TResult}"/> instance wrapping a dynamic method to read the field value (with implicit hierarchy traversal)
        /// </summary>
        public Func<object, object> Getter { get; private set; }

        public ClosureFieldWithUnwrappedGetter(FieldInfo info, IReadOnlyList<FieldInfo> parents)
        {
            Info = info;
            Parents = parents;
        }

        /// <summary>
        /// Preloads the dynamic method used to retrieve the value of the wrapped <see cref="FieldInfo"/> instance
        /// </summary>
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

