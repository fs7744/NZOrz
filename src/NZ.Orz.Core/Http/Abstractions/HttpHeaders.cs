using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace NZ.Orz.Http;

public partial class HttpHeaders : IHeaderDictionary
{
    private readonly Dictionary<string, StringValues> dict;

    public HttpHeaders()
    {
        dict = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
    }

    public HttpHeaders(int capacity)
    {
        dict = new Dictionary<string, StringValues>(capacity, StringComparer.OrdinalIgnoreCase);
    }

    private int _FastCount;
    public int Count => _FastCount + dict.Count;

    public StringValues this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void Clear()
    {
        FastClear();
        dict.Clear();
    }

    public void Add(string key, StringValues value)
    {
        var k = HeaderNames.GetInternedHeaderName(key);
        if (!FastAdd(k, value))
        {
            dict.Add(k, value);
        }
    }

    public bool ContainsKey(string key)
    {
        return TryGetValue(key, out _);
    }

    public bool Remove(string key)
    {
        var k = HeaderNames.GetInternedHeaderName(key);
        if (FastRemove(k))
        {
            return true;
        }
        else
        {
            return dict.Remove(k);
        }
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value)
    {
        var k = HeaderNames.GetInternedHeaderName(key);
        if (FastTryGetValue(k, out value))
        {
            return true;
        }
        else
        {
            return dict.TryGetValue(key, out value);
        }
    }

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}