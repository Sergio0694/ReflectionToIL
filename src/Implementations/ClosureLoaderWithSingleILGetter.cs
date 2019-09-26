using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ReflectionToIL.Models;

namespace ReflectionToIL.Implementations
{
    /// <summary>
    /// A custom <see langword="delegate"/> that takes an instance of a closure class and extracts all the captured variables
    /// </summary>
    /// <param name="instance">The closure class instance to read data from</param>
    /// <param name="r0">A reference to an <see cref="object"/> array to load reference types</param>
    /// <param name="r1">A reference to a <see cref="byte"/> array to serialize value types</param>
    public delegate void DataLoader(object instance, ref object r0, ref byte r1);

    /// <summary>
    /// A <see langword="class"/> that inspects a closure and loads fields with a single dynamic method doing all the work
    /// </summary>
    public sealed class ClosureLoaderWithSingleILGetter
    {
        /// <summary>
        /// The number of captured variables of a reference type
        /// </summary>
        private readonly int ReferenceCount;

        /// <summary>
        /// The size in bytes of all the captured variables of a value type
        /// </summary>
        private readonly int ByteSize;

        /// <summary>
        /// The <see cref="DataLoader"/> instance to use to extract data from new closure instances
        /// </summary>
        private readonly DataLoader Loader;

        private ClosureLoaderWithSingleILGetter(int references, int bytes, DataLoader loader)
        {
            ReferenceCount = references;
            ByteSize = bytes;
            Loader = loader;
        }

        /// <summary>
        /// Creates a new <see cref="ClosureLoaderWithSingleILGetter"/> instance after inspecting the given <see cref="Delegate"/>
        /// </summary>
        /// <param name="instance">The input <see cref="Delegate"/> instance to inspect</param>
        /// <returns>A <see cref="ClosureLoaderWithSingleILGetter"/> instance that can be used to load data from instances of the same closure class as the input</returns>
        public static ClosureLoaderWithSingleILGetter GetLoaderForDelegate(Delegate instance)
        {
            IEnumerable<ClosureField> CollectFields(Type type, IReadOnlyList<FieldInfo> parents)
            {
                foreach (FieldInfo field in type.GetFields())
                {
                    if (field.Name.StartsWith("CS$<>"))
                    {
                        // Create a new list of parents including the current field
                        IReadOnlyList<FieldInfo> updatedParents = parents.Concat(new[] { field }).ToList();

                        foreach (var nested in CollectFields(field.FieldType, updatedParents))
                        {
                            yield return nested;
                        }
                    }
                    else yield return new ClosureField(field, parents);
                }
            }

            // Explore the closure type, initially passing an empty array of parents
            var fields = CollectFields(instance.Target.GetType(), Array.Empty<FieldInfo>()).ToArray();

            // Prepare the info for our custom delegate
            Type returnType = typeof(void);
            Type[] parameterTypes =
            {
                typeof(object),
                typeof(object).MakeByRefType(),
                typeof(byte).MakeByRefType()
            };

            // Create a new dynamic method
            Type ownerType = instance.GetType();
            DynamicMethod method = new DynamicMethod(
                $"GetFor{ownerType.Name}",
                returnType,
                parameterTypes,
                ownerType);
            ILGenerator il = method.GetILGenerator();

            // Mapping of parent members to index of the relative local variable
            var map =
                fields
                .Where(m => m.Parents.Count > 0)
                .SelectMany(m => m.Parents)
                .Distinct()
                .Select((m, i) => (Member: m, Index: i))
                .ToDictionary(p => (object)p.Member, p => p.Index + 1);
            object root = new object(); // Placeholder
            map.Add(root, 0);

            // Set of indices of the loaded parent members
            HashSet<int> loaded = new HashSet<int>(new[] { 0 });

            // Loads a given member on the top of the execution stack
            void LoadMember(ClosureField member)
            {
                // Load the parent instance on the execution stack
                int index = map[member.Parents.LastOrDefault() ?? root];
                if (loaded.Contains(index)) il.EmitLoadLocal(index);
                else
                {
                    // Seek upwards to find the most in depth loaded parent
                    int i = member.Parents.Count - 1;
                    while (i > 0 && !loaded.Contains(map[member.Parents[i]])) i--;

                    // Load the local variables for all the parents of the current member
                    il.EmitLoadLocal(0);
                    for (; i < member.Parents.Count; i++)
                    {
                        index = map[member.Parents[i]];
                        il.Emit(OpCodes.Ldfld, member.Parents[i]);
                        il.EmitStoreLocal(index);
                        il.EmitLoadLocal(index);
                        loaded.Add(index);
                    }
                }

                // Finally load the field from the object on the stack
                il.Emit(OpCodes.Ldfld, member.Info);
            }

            // Declare the local variables
            il.DeclareLocal(instance.Method.DeclaringType);
            foreach (FieldInfo field in map.OrderBy(p => p.Value).Skip(1).Select(p => p.Key))
            {
                il.DeclareLocal(field.FieldType);
            }

            // Cast the closure instance and assign it to the local variable
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, instance.Method.DeclaringType);
            il.Emit(OpCodes.Stloc_0);

            // Handle all the captured fields, both objects and value types
            int
                referenceOffset = 0, // Offset into the references array
                byteOffset = 0;      // Offset into the bytes array
            foreach (ClosureField field in fields)
            {
                if (field.Info.FieldType.IsValueType)
                {
                    il.Emit(OpCodes.Ldarg_2); // Load ref byte r1
                    il.EmitAddOffset(byteOffset);   // Offset the reference

                    byteOffset += Marshal.SizeOf(field.Info.FieldType);
                }
                else
                {
                    // Load the offset address into the resource buffers
                    il.Emit(OpCodes.Ldarg_1);
                    if (referenceOffset > 0)
                    {
                        int offset = Unsafe.SizeOf<object>() * referenceOffset;
                        il.EmitAddOffset(offset);
                    }

                    referenceOffset++;
                }

                /* Load the current member accordingly.
                 * When this method returns, the value of the current
                 * member will be at the top of the execution stack */
                LoadMember(field);

                il.EmitStoreToAddress(field.Info.FieldType);
            }

            il.Emit(OpCodes.Ret);

            // Create the proper delegate type for the method
            DataLoader loader = (DataLoader)method.CreateDelegate(typeof(DataLoader));

            return new ClosureLoaderWithSingleILGetter(referenceOffset, byteOffset, loader);
        }

        /// <summary>
        /// Loads all the captured variables from a given closure class and returns them as a <see cref="ClosureData"/> instance
        /// </summary>
        /// <param name="instance">The input <see cref="Delegate"/> instance to load data from</param>
        /// <returns>A <see cref="ClosureData"/> instance with the captured data</returns>
        public unsafe ClosureData GetData(Delegate instance)
        {
            // Reference and byte array
            object[] refs = ArrayPool<object>.Shared.Rent(ReferenceCount);
            byte[] bytes = ArrayPool<byte>.Shared.Rent(ByteSize);
            ref object r0 = ref refs.Length > 0 ? ref refs[0] : ref Unsafe.AsRef<object>(null);
            ref byte r1 = ref bytes.Length > 0 ? ref bytes[0] : ref Unsafe.AsRef<byte>(null);

            // Invoke the dynamic method to extract the captured data
            Loader(instance.Target, ref r0, ref r1);

            return new ClosureData(refs, ReferenceCount, bytes, ByteSize);
        }
    }
}
