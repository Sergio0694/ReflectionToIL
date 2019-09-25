using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ReflectionToIL.Models;

namespace ReflectionToIL.Implementations
{
    /// <summary>
    /// A <see langword="class"/> that inspects a closure and loads fields using only reflection
    /// </summary>
    public sealed class ClosureLoaderWithReflection
    {
        /// <summary>
        /// The list of <see cref="ClosureField"/> instances mapping the captured variables in the current closure class
        /// </summary>
        private readonly IReadOnlyList<ClosureField> Fields;

        /// <summary>
        /// The number of captured variables of a reference type
        /// </summary>
        private readonly int ReferenceCount;

        /// <summary>
        /// The size in bytes of all the captured variables of a value type
        /// </summary>
        private readonly int ByteSize;

        private ClosureLoaderWithReflection(IReadOnlyList<ClosureField> fields, int references, int bytes)
        {
            Fields = fields;
            ReferenceCount = references;
            ByteSize = bytes;
        }

        /// <summary>
        /// Creates a new <see cref="ClosureLoaderWithReflection"/> instance after inspecting the given <see cref="Delegate"/>
        /// </summary>
        /// <param name="instance">The input <see cref="Delegate"/> instance to inspect</param>
        /// <returns>A <see cref="ClosureLoaderWithReflection"/> instance that can be used to load data from instances of the same closure class as the input</returns>
        public static ClosureLoaderWithReflection GetLoaderForDelegate(Delegate instance)
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

            // Calculate how many bytes we need
            int
                referenceCount = 0,
                byteSize = 0;
            foreach (var field in fields)
            {
                if (field.Info.FieldType.IsValueType) byteSize += Marshal.SizeOf(field.Info.FieldType);
                else referenceCount++;
            }

            return new ClosureLoaderWithReflection(fields, referenceCount, byteSize);
        }

        /// <summary>
        /// Loads all the captured variables from a given closure class and returns them as a <see cref="ClosureData"/> instance
        /// </summary>
        /// <param name="instance">The input <see cref="Delegate"/> instance to load data from</param>
        /// <returns>A <see cref="ClosureData"/> instance with the captured data</returns>
        public unsafe ClosureData GetData(Delegate instance)
        {
            object[] references = ArrayPool<object>.Shared.Rent(ReferenceCount);
            byte[] bytes = ArrayPool<byte>.Shared.Rent(ByteSize);
            int
                referenceOffset = 0, // Offset into the references array
                byteOffset = 0;      // Offset into the bytes array 

            for (int i = 0; i < Fields.Count; i++) // Faster than foreach
            {
                ClosureField field = Fields[i];

                // Traverse the parents hierarchy
                object target = instance.Target;
                for (int j = 0; j < field.Parents.Count; j++)
                    target = field.Parents[i].GetValue(target);
                object value = field.Info.GetValue(target);

                // We need to handle value types and objects differently
                if (field.Info.FieldType.IsValueType)
                {
                    // Pin the boxed value and get the source and destination references
                    GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
                    void* p = handle.AddrOfPinnedObject().ToPointer();
                    ref byte source = ref Unsafe.AsRef<byte>(p);
                    ref byte destination = ref Unsafe.Add(ref bytes[0], byteOffset);
                    int size = Marshal.SizeOf(field.Info.FieldType);

                    // Copy the raw data of the value type into our bytes buffer
                    Unsafe.CopyBlock(ref destination, ref source, (uint)size);
                    byteOffset += size;

                    handle.Free(); // Do NOT forget to do this
                }
                else references[referenceOffset++] = value;
            }

            return new ClosureData(references, ReferenceCount, bytes, ByteSize);
        }
    }
}
