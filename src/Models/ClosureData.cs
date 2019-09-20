using System.Buffers;

namespace ReflectionToIL.Models
{
    public readonly ref struct ClosureData
    {
        private readonly object[] References;

        private readonly byte[] Bytes;

        public ClosureData(object[] references, byte[] bytes)
        {
            References = references;
            Bytes = bytes;
        }

        public void Dispose()
        {
            ArrayPool<object>.Shared.Return(References);
            ArrayPool<byte>.Shared.Return(Bytes);
        }
    }
}
