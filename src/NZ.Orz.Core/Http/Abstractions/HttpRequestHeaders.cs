using Microsoft.Extensions.Primitives;
using NZ.Orz.Http.Exceptions;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NZ.Orz.Http;

public partial class HttpRequestHeaders : IHeaderDictionary
{
    private ulong _bits;
    private HeaderReferences _r = new HeaderReferences();
    private Dictionary<string, StringValues> dict = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
    internal long? _contentLength;

    public int Count => BitOperations.PopCount(_bits) + (dict == null ? 0 : dict.Count);

    public bool IsReadOnly => false;

    public StringValues this[string key]
    {
        get
        {
            TryGetValue(key, out var value);
            return value;
        }
        set
        {
            if (GetInternedHeaderType(key, out var k))
            {
                FastAdd(k, value);
            }
            else
            {
                dict[key] = value;
            }
        }
    }

    public void Clear()
    {
        FastClear();
        dict.Clear();
    }

    public void Add(string key, StringValues value)
    {
        if (GetInternedHeaderType(key, out var k))
        {
            FastAdd(k, value);
        }
        else
        {
            dict.Add(key, value);
        }
    }

    public bool ContainsKey(string key)
    {
        return TryGetValue(key, out _);
    }

    public bool Remove(string key)
    {
        if (GetInternedHeaderType(key, out var k))
        {
            return FastRemove(k);
        }
        else
        {
            return dict.Remove(key);
        }
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value)
    {
        if (GetInternedHeaderType(key, out var k))
        {
            return FastTryGetValue(k, out value);
        }
        else
        {
            return dict.TryGetValue(key, out value);
        }
    }

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    public void Add(KeyValuePair<string, StringValues> item)
    {
        Add(item.Key, item.Value);
    }

    public bool Contains(KeyValuePair<string, StringValues> item)
    {
        return ContainsKey(item.Key);
    }

    public bool Remove(KeyValuePair<string, StringValues> item)
    {
        return Remove(item.Key);
    }

    public partial struct Enumerator : IEnumerator<KeyValuePair<string, StringValues>>
    {
        private readonly HttpRequestHeaders _collection;
        private KeyValuePair<string, StringValues> _current;
        private int _next;
        private ulong _currentBits;
        private readonly IEnumerator<KeyValuePair<string, StringValues>> _enumerator;

        internal Enumerator(HttpRequestHeaders collection)
        {
            _collection = collection;
            _enumerator = _collection.dict.GetEnumerator();
            _currentBits = collection._bits;
            _next = GetNext(_currentBits);
        }

        public KeyValuePair<string, StringValues> Current => _current;

        object IEnumerator.Current => _current;

        public void Dispose()
        {
        }

        public void Reset()
        {
            _currentBits = _collection._bits;
            _next = GetNext(_currentBits);
            _enumerator?.Reset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNext(ulong bits)
        {
            return bits != 0
                ? BitOperations.TrailingZeroCount(bits)
                : -1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetInternedHeaderType(string name, out KnownHeaderType type)
    {
        if (_internedHeaderType.TryGetValue(name, out type))
        {
            return true;
        }

        return false;
    }

    private static long ParseContentLength(string value)
    {
        if (!HeaderUtilities.TryParseNonNegativeInt64(value, out var parsed))
        {
            throw BadHttpRequestException.GetException(RequestRejectionReason.InvalidContentLength, value);
        }

        return parsed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUnalignedLittleEndian_ushort(ref byte source)
    {
        ushort result = Unsafe.ReadUnaligned<ushort>(ref source);
        if (!BitConverter.IsLittleEndian)
        {
            result = BinaryPrimitives.ReverseEndianness(result);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUnalignedLittleEndian_uint(ref byte source)
    {
        uint result = Unsafe.ReadUnaligned<uint>(ref source);
        if (!BitConverter.IsLittleEndian)
        {
            result = BinaryPrimitives.ReverseEndianness(result);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUnalignedLittleEndian_ulong(ref byte source)
    {
        ulong result = Unsafe.ReadUnaligned<ulong>(ref source);
        if (!BitConverter.IsLittleEndian)
        {
            result = BinaryPrimitives.ReverseEndianness(result);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AppendContentLength(ReadOnlySpan<byte> value)
    {
        if (!Utf8Parser.TryParse(value, out long parsed, out var consumed) ||
            parsed < 0 ||
            consumed != value.Length)
        {
            throw BadHttpRequestException.GetException(RequestRejectionReason.InvalidContentLength, value.GetRequestHeaderString(HeaderNames.ContentLength, checkForNewlineChars: false));
        }

        _contentLength = parsed;
    }

    //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
    //public void Append(ReadOnlySpan<byte> name, ReadOnlySpan<byte> value, bool checkForNewlineChars)
    //{
    //    var nameStr = string.Empty;
    //    ref byte nameStart = ref MemoryMarshal.GetReference(name);
    //    ref StringValues values = ref Unsafe.NullRef<StringValues>();
    //    var flag = 0UL;
    //    switch (name.Length)
    //    {
    //        case 4:
    //            var n = ReadUnalignedLittleEndian_uint(ref nameStart);
    //            nameStart = Unsafe.AddByteOffset(ref nameStart, (IntPtr)2);
    //            if (n == 1953722184U)
    //            {
    //                flag = 1UL;
    //                values = ref _r.Host;
    //                nameStr = HeaderNames.Host;
    //            }
    //            break;

    //        default:
    //            break;
    //    }
    //    if (flag != 0UL)
    //    {
    //        var valueStr = value.GetRequestHeaderString(nameStr, checkForNewlineChars);
    //        if ((_bits & flag) == 0)
    //        {
    //            _bits |= flag;
    //            values = new StringValues(valueStr);
    //        }
    //        else
    //        {
    //            values = StringValues.Concat(values, valueStr);
    //        }
    //    }
    //    else
    //    {
    //        nameStr = name.GetHeaderName();
    //        var valueStr = value.GetRequestHeaderString(nameStr, checkForNewlineChars);
    //        dict.TryGetValue(nameStr, out var existing);
    //        dict[nameStr] = StringValues.Concat(existing, valueStr);
    //    }
    //}
}