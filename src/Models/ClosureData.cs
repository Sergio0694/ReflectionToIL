using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace ReflectionToIL.Models
{
    /// <summary>
    /// A <see langword="struct"/> that wraps the data extracted from a given closure field
    /// </summary>
    public readonly ref struct ClosureData
    {
        // The rented array with the loaded reference types
        private readonly object[] _References;

        // The number of valid items in the reference types array
        private readonly int ReferencesCount;

        // The rented array with the loaded value types
        private readonly byte[] _Bytes;

        // The number of valid items in the value types array
        private readonly int BytesCount;

        public ClosureData(object[] references, int referencesCount, byte[] bytes, int bytesCount)
        {
            _References = references;
            ReferencesCount = referencesCount;
            _Bytes = bytes;
            BytesCount = bytesCount;
        }

        /// <summary>
        /// Gets a <see cref="Span{T}"/> with the loaded reference type variables
        /// </summary>
        public Span<object> References
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _References.AsSpan(0, ReferencesCount);
        }

        /// <summary>
        /// Gets a <see cref="Span{T}"/> with the loaded value type variables
        /// </summary>
        public Span<byte> Bytes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Bytes.AsSpan(0, BytesCount);
        }

        /// <inheritdoc cref="IDisposable"/>
        public void Dispose()
        {
            ArrayPool<object>.Shared.Return(_References);
            ArrayPool<byte>.Shared.Return(_Bytes);
        }
    }
}
