using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace ReflectionToIL.Models
{
    public readonly ref struct ClosureData
    {
        private readonly object[] _References;

        private readonly int ReferencesCount;

        private readonly byte[] _Bytes;

        private readonly int BytesCount;

        public ClosureData(object[] references, int referencesCount, byte[] bytes, int bytesCount)
        {
            _References = references;
            ReferencesCount = referencesCount;
            _Bytes = bytes;
            BytesCount = bytesCount;
        }

        public Span<object> References
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _References.AsSpan(0, ReferencesCount);
        }

        public Span<byte> Bytes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Bytes.AsSpan(0, BytesCount);
        }

        public void Dispose()
        {
            ArrayPool<object>.Shared.Return(_References);
            ArrayPool<byte>.Shared.Return(_Bytes);
        }
    }
}
