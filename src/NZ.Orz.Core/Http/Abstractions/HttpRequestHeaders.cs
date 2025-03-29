using Microsoft.Extensions.Primitives;
using NZ.Orz.Http.Exceptions;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace NZ.Orz.Http;

public partial class HttpRequestHeaders : IHeaderDictionary
{
    private ulong _bits;
    private HeaderReferences _r = new HeaderReferences();
    private Dictionary<string, StringValues> dict;
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
                if (dict == null)
                {
                    dict = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
                }
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
            _enumerator = _collection.dict?.GetEnumerator();
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
}