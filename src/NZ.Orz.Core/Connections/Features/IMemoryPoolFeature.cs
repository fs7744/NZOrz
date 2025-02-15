using System.Buffers;

namespace NZ.Orz.Connections.Features;

public interface IMemoryPoolFeature
{
    MemoryPool<byte> MemoryPool { get; }
}