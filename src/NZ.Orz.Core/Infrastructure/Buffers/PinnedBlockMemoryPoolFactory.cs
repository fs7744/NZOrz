using System.Buffers;

namespace NZ.Orz.Buffers;

internal class PinnedBlockMemoryPoolFactory
{
    public static MemoryPool<byte> Create()
    {
        return new PinnedBlockMemoryPool();
    }
}