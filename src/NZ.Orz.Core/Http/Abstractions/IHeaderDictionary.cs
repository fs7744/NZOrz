using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace NZ.Orz.Http;

public interface IHeaderDictionary : IEnumerable<KeyValuePair<string, StringValues>>, ICollection<KeyValuePair<string, StringValues>>
{
    int Count { get; }
    StringValues this[string key] { get; set; }

    void Add(string key, StringValues value);

    bool ContainsKey(string key);

    bool Remove(string key);

    bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value);

    void Clear();
}