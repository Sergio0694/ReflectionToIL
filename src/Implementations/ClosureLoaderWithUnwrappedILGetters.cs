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
    public sealed class ClosureLoaderWithUnwrappedILGetters
    {
        private readonly IReadOnlyList<ClosureFieldWithUnwrappedGetter> Fields;

        private readonly int ReferenceCount;

        private readonly int ByteSize;

        private ClosureLoaderWithUnwrappedILGetters(IReadOnlyList<ClosureFieldWithUnwrappedGetter> fields, int references, int bytes)
        {
            Fields = fields;
            ReferenceCount = references;
            ByteSize = bytes;
        }

        public static ClosureLoaderWithUnwrappedILGetters GetLoaderForDelegate(Delegate instance)
        {
            IEnumerable<ClosureFieldWithUnwrappedGetter> CollectFields(Type type, IReadOnlyList<FieldInfo> parents)
            {
                foreach (FieldInfo field in type.GetFields())
                {
                    if (field.Name.StartsWith("CS$<>"))
                    {
                        // Create a new list of parents including the current field
                        IReadOnlyList<FieldInfo> updatedParents = parents.Append(field).ToList();

                        foreach (var nested in CollectFields(field.FieldType, updatedParents))
                        {
                            yield return nested;
                        }
                    }
                    else yield return new ClosureFieldWithUnwrappedGetter(field, parents);
                }
            }

            // Explore the closure type, initially passing an empty array of parents
            var fields = CollectFields(instance.Target.GetType(), Array.Empty<FieldInfo>()).ToArray();

            // Preload the dynamic getters
            foreach (var field in fields)
            {
                field.BuildDynamicGetter();
            }

            // Calculate how many bytes we need
            int
                referenceCount = 0,
                byteSize = 0;
            foreach (var field in fields)
            {
                if (field.Info.FieldType.IsValueType) byteSize += Marshal.SizeOf(field.Info.FieldType);
                else referenceCount++;
            }

            return new ClosureLoaderWithUnwrappedILGetters(fields, referenceCount, byteSize);
        }

        public unsafe ClosureData GetData(Delegate instance)
        {
            object[] references = ArrayPool<object>.Shared.Rent(ReferenceCount);
            byte[] bytes = ArrayPool<byte>.Shared.Rent(ByteSize);
            int
                referenceOffset = 0, // Offset into the references array
                byteOffset = 0;      // Offset into the bytes array

            for (int i = 0; i < Fields.Count; i++)
            {
                // Load the current field
                object value = Fields[i].Getter(instance.Target);

                // We need to handle value types and objects differently
                if (Fields[i].Info.FieldType.IsValueType)
                {
                    // Pin the boxed value and get the source and destination references
                    GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
                    void* p = handle.AddrOfPinnedObject().ToPointer();
                    ref byte source = ref Unsafe.AsRef<byte>(p);
                    ref byte destination = ref Unsafe.Add(ref bytes[0], byteOffset);
                    int size = Marshal.SizeOf(Fields[i].Info.FieldType);

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
