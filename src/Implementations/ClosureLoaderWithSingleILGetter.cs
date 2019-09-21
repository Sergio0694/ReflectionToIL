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
    public delegate void DataLoader(object instance, ref object r0, ref byte r1);

    public sealed class ClosureLoaderWithSingleILGetter
    {
        private readonly int ReferenceCount;

        private readonly int ByteSize;

        private readonly DataLoader Loader;

        private ClosureLoaderWithSingleILGetter(int references, int bytes, DataLoader loader)
        {
            ReferenceCount = references;
            ByteSize = bytes;
            Loader = loader;
        }

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
                    if (i == 0) il.EmitLoadLocal(0);
                    for (; i < member.Parents.Count; i++)
                    {
                        il.Emit(OpCodes.Ldfld, member.Parents[i]);
                        il.EmitStoreLocal(map[member.Parents[i]]);
                        il.EmitLoadLocal(map[member.Parents[i]]);
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
