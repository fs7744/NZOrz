using Microsoft.Extensions.Primitives;
using NZ.Orz.Http.Exceptions;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
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
    public int HostCount => _r.Host.Count;

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

    private static readonly string KeepAliveStr = "keep-alive";
#pragma warning restore CA1802
    private static readonly StringValues ConnectionValueKeepAlive = KeepAliveStr;
    private static readonly StringValues ConnectionValueClose = "close";
    private static readonly StringValues ConnectionValueUpgrade = HeaderNames.Upgrade;

    public static ConnectionOptions ParseConnection(HttpRequestHeaders headers)
    {
        // Keep-alive
        const ulong lowerCaseKeep = 0x0000_0020_0020_0020; // Don't lowercase hyphen
        const ulong keepAliveStart = 0x002d_0070_0065_0065; // 4 chars "eep-"
        const ulong keepAliveMiddle = 0x0076_0069_006c_0061; // 4 chars "aliv"
        const ushort keepAliveEnd = 0x0065; // 1 char "e"
                                            // Upgrade
        const ulong upgradeStart = 0x0061_0072_0067_0070; // 4 chars "pgra"
        const uint upgradeEnd = 0x0065_0064; // 2 chars "de"
                                             // Close
        const ulong closeEnd = 0x0065_0073_006f_006c; // 4 chars "lose"

        var connection = headers.Connection;
        var connectionCount = connection.Count;
        if (connectionCount == 0)
        {
            return ConnectionOptions.None;
        }

        var connectionOptions = ConnectionOptions.None;

        if (connectionCount == 1)
        {
            // "keep-alive" is the only value that will be repeated over
            // many requests on the same connection; on the first request
            // we will have switched it for the readonly static value;
            // so we can ptentially short-circuit parsing and use ReferenceEquals.
            if (ReferenceEquals(connection.ToString(), KeepAliveStr))
            {
                return ConnectionOptions.KeepAlive;
            }
        }

        for (var i = 0; i < connectionCount; i++)
        {
            var value = connection[i].AsSpan();
            while (value.Length > 0)
            {
                int offset;
                char c = '\0';
                // Skip any spaces and empty values.
                for (offset = 0; offset < value.Length; offset++)
                {
                    c = value[offset];
                    if (c != ' ' && c != ',')
                    {
                        break;
                    }
                }

                // Skip last read char.
                offset++;
                if ((uint)offset > (uint)value.Length)
                {
                    // Consumed enitre string, move to next.
                    break;
                }

                // Remove leading spaces or empty values.
                value = value.Slice(offset);
                c = ToLowerCase(c);

                var byteValue = MemoryMarshal.AsBytes(value);

                offset = 0;
                var potentialConnectionOptions = ConnectionOptions.None;

                if (c == 'k' && byteValue.Length >= (2 * sizeof(ulong) + sizeof(ushort)))
                {
                    if (ReadLowerCaseUInt64(byteValue, lowerCaseKeep) == keepAliveStart)
                    {
                        offset += sizeof(ulong) / 2;
                        byteValue = byteValue.Slice(sizeof(ulong));

                        if (ReadLowerCaseUInt64(byteValue) == keepAliveMiddle)
                        {
                            offset += sizeof(ulong) / 2;
                            byteValue = byteValue.Slice(sizeof(ulong));

                            if (ReadLowerCaseUInt16(byteValue) == keepAliveEnd)
                            {
                                offset += sizeof(ushort) / 2;
                                potentialConnectionOptions = ConnectionOptions.KeepAlive;
                            }
                        }
                    }
                }
                else if (c == 'u' && byteValue.Length >= (sizeof(ulong) + sizeof(uint)))
                {
                    if (ReadLowerCaseUInt64(byteValue) == upgradeStart)
                    {
                        offset += sizeof(ulong) / 2;
                        byteValue = byteValue.Slice(sizeof(ulong));

                        if (ReadLowerCaseUInt32(byteValue) == upgradeEnd)
                        {
                            offset += sizeof(uint) / 2;
                            potentialConnectionOptions = ConnectionOptions.Upgrade;
                        }
                    }
                }
                else if (c == 'c' && byteValue.Length >= sizeof(ulong))
                {
                    if (ReadLowerCaseUInt64(byteValue) == closeEnd)
                    {
                        offset += sizeof(ulong) / 2;
                        potentialConnectionOptions = ConnectionOptions.Close;
                    }
                }

                if ((uint)offset >= (uint)value.Length)
                {
                    // Consumed enitre string, move to next string.
                    connectionOptions |= potentialConnectionOptions;
                    break;
                }
                else
                {
                    value = value.Slice(offset);
                }

                for (offset = 0; offset < value.Length; offset++)
                {
                    c = value[offset];
                    if (c == ',')
                    {
                        break;
                    }
                    else if (c != ' ')
                    {
                        // Value contains extra chars; this is not the matched one.
                        potentialConnectionOptions = ConnectionOptions.None;
                    }
                }

                if ((uint)offset >= (uint)value.Length)
                {
                    // Consumed enitre string, move to next string.
                    connectionOptions |= potentialConnectionOptions;
                    break;
                }
                else if (c == ',')
                {
                    // Consumed value corretly.
                    connectionOptions |= potentialConnectionOptions;
                    // Skip comma.
                    offset++;
                    if ((uint)offset >= (uint)value.Length)
                    {
                        // Consumed enitre string, move to next string.
                        break;
                    }
                    else
                    {
                        // Move to next value.
                        value = value.Slice(offset);
                    }
                }
            }
        }

        // If Connection is a single value, switch it for the interned value
        // in case the connection is long lived
        if (connectionOptions == ConnectionOptions.Upgrade)
        {
            headers.Connection = ConnectionValueUpgrade;
        }
        else if (connectionOptions == ConnectionOptions.KeepAlive)
        {
            headers.Connection = ConnectionValueKeepAlive;
        }
        else if (connectionOptions == ConnectionOptions.Close)
        {
            headers.Connection = ConnectionValueClose;
        }

        return connectionOptions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ReadLowerCaseUInt64(ReadOnlySpan<byte> value, ulong lowerCaseMask = 0x0020_0020_0020_0020)
    {
        ulong result = MemoryMarshal.Read<ulong>(value);
        if (!BitConverter.IsLittleEndian)
        {
            result = (result & 0xffff_0000_0000_0000) >> 48 |
                     (result & 0x0000_ffff_0000_0000) >> 16 |
                     (result & 0x0000_0000_ffff_0000) << 16 |
                     (result & 0x0000_0000_0000_ffff) << 48;
        }
        return result | lowerCaseMask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ReadLowerCaseUInt32(ReadOnlySpan<byte> value)
    {
        uint result = MemoryMarshal.Read<uint>(value);
        if (!BitConverter.IsLittleEndian)
        {
            result = (result & 0xffff_0000) >> 16 |
                     (result & 0x0000_ffff) << 16;
        }
        return result | 0x0020_0020;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort ReadLowerCaseUInt16(ReadOnlySpan<byte> value)
        => (ushort)(MemoryMarshal.Read<ushort>(value) | 0x0020);

    private static char ToLowerCase(char value) => (char)(value | (char)0x0020);

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