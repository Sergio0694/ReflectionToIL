using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
    /// <summary>
    /// A <see langword="class"/> that provides extension methods for the <see langword="ILGenerator"/> type
    /// </summary>
    public static class ILGeneratorExtensions
    {
        /// <summary>
        /// Emits the appropriate <see langword="ldloc"/> instruction to read a local variable
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the local variable to load</param>
        public static void EmitLoadLocal(this ILGenerator il, int index)
        {
            if (index <= 3)
            {
                il.Emit(index switch
                {
                    0 => OpCodes.Ldloc_0,
                    1 => OpCodes.Ldloc_1,
                    2 => OpCodes.Ldloc_2,
                    3 => OpCodes.Ldloc_3,
                    _ => throw new InvalidOperationException($"Invalid local variable index [{index}]")
                });
            }
            else if (index <= 255) il.Emit(OpCodes.Ldloc_S, (byte)index);
            else if (index <= 65534) il.Emit(OpCodes.Ldloc, (short)index);
            else throw new ArgumentOutOfRangeException($"Invalid local index {index}");
        }

        /// <summary>
        /// Emits the appropriate <see langword="stloc"/> instruction to write a local variable
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="index">The index of the local variable to store</param>
        public static void EmitStoreLocal(this ILGenerator il, int index)
        {
            if (index <= 3)
            {
                il.Emit(index switch
                {
                    0 => OpCodes.Stloc_0,
                    1 => OpCodes.Stloc_1,
                    2 => OpCodes.Stloc_2,
                    3 => OpCodes.Stloc_3,
                    _ => throw new InvalidOperationException($"Invalid local variable index [{index}]")
                });
            }
            else if (index <= 255) il.Emit(OpCodes.Stloc_S, (byte)index);
            else if (index <= 65534) il.Emit(OpCodes.Stloc, (short)index);
            else throw new ArgumentOutOfRangeException($"Invalid local index {index}");
        }

        /// <summary>
        /// Emits the appropriate <see langword="ldc.i4"/>, <see langword="conv.i"/> and <see langword="add"/> instructions to advance a reference
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="offset">The offset in bytes to use to advance the current reference on top of the execution stack</param>
        public static void EmitAddOffset(this ILGenerator il, int offset)
        {
            // Push the offset to the stack
            if (offset <= 8)
            {
                il.Emit(offset switch
                {
                    0 => OpCodes.Ldc_I4_0,
                    1 => OpCodes.Ldc_I4_1,
                    2 => OpCodes.Ldc_I4_2,
                    3 => OpCodes.Ldc_I4_3,
                    4 => OpCodes.Ldc_I4_4,
                    5 => OpCodes.Ldc_I4_5,
                    6 => OpCodes.Ldc_I4_6,
                    7 => OpCodes.Ldc_I4_7,
                    8 => OpCodes.Ldc_I4_8,
                    _ => throw new InvalidOperationException($"Invalid offset value [{offset}]")
                });
            }
            else if (offset <= 127) il.Emit(OpCodes.Ldc_I4_S, (byte)offset);
            else il.Emit(OpCodes.Ldc_I4, offset);

            il.Emit(OpCodes.Conv_I);    // Convert the int to native int (void*)
            il.Emit(OpCodes.Add);       // Pop the two values, sum them and push the result
        }

        /// <summary>
        /// Emits the appropriate <see langword="stind"/> or <see langword="stobj"/> instruction to write to a reference
        /// </summary>
        /// <param name="il">The input <see cref="ILGenerator"/> instance to use to emit instructions</param>
        /// <param name="type">The type of value being written to the current reference on top of the execution stack</param>
        public static void EmitStoreToAddress(this ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                // Pick the optimal opcode to set a value type
                OpCode opcode = Marshal.SizeOf(type) switch
                {
                    // Use the faster op codes for sizes <= 8
                    1 when type == typeof(byte) || type == typeof(sbyte) => OpCodes.Stind_I1,
                    2 when type == typeof(short) || type == typeof(ushort) => OpCodes.Stind_I2,
                    4 when type == typeof(float) => OpCodes.Stind_R4,
                    4 when type == typeof(int) || type == typeof(uint) => OpCodes.Stind_I4,
                    8 when type == typeof(double) => OpCodes.Stind_R8,
                    8 when type == typeof(long) || type == typeof(ulong) => OpCodes.Stind_I8,

                    // Default to stobj for all other value types
                    _ => OpCodes.Stobj
                };

                // Also pass the type as argument if stobj is used
                if (opcode == OpCodes.Stobj) il.Emit(opcode, type);
                else il.Emit(opcode);
            }
            else il.Emit(OpCodes.Stind_Ref);
        }
    }
}
