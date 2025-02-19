using System.Buffers;

namespace NZ.Orz.Buffers;

internal class PinnedBlockMemoryPoolFactory
{
    public static MemoryPool<byte> Create()
    {
        return Create(4096);
    }

    public static MemoryPool<byte> Create(int blockSize)
    {
        return new PinnedBlockMemoryPool(blockSize);
    }
}