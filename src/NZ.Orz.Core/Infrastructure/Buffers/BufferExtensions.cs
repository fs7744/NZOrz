using System.Runtime.InteropServices;

namespace NZ.Orz.Buffers;

public static class BufferExtensions
{
    public static ArraySegment<byte> GetArray(this Memory<byte> memory)
    {
        return ((ReadOnlyMemory<byte>)memory).GetArray();
    }

    public static ArraySegment<byte> GetArray(this ReadOnlyMemory<byte> memory)
    {
        if (!MemoryMarshal.TryGetArray(memory, out var result))
        {
            throw new InvalidOperationException("Buffer backed by array was expected");
        }
        return result;
    }
}