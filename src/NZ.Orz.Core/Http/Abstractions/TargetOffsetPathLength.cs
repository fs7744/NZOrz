using System.Runtime.CompilerServices;

namespace NZ.Orz.Http;

public readonly struct TargetOffsetPathLength
{
    private readonly ulong _targetOffsetPathLength;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TargetOffsetPathLength(int offset, int length, bool isEncoded)
    {
        if (isEncoded)
        {
            length = -length;
        }

        _targetOffsetPathLength = ((ulong)offset << 32) | (uint)length;
    }

    public int Offset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return (int)(_targetOffsetPathLength >> 32);
        }
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var length = (int)_targetOffsetPathLength;
            if (length < 0)
            {
                length = -length;
            }

            return length;
        }
    }

    public bool IsEncoded
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return (int)_targetOffsetPathLength < 0 ? true : false;
        }
    }
}