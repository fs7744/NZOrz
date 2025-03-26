using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

public interface IHeaderDictionary : IEnumerable<KeyValuePair<string, StringValues>>
{
    int Count { get; }
    StringValues this[string key] { get; set; }

    void Add(string key, StringValues value);

    bool ContainsKey(string key);

    bool Remove(string key);

    bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value);

    void Clear();
}

public class HttpRequestHeaders : HttpHeaders
{
}