using DotNext.Collections.Generic;
using DotNext.Runtime.Caching;
using System.Collections.Frozen;

namespace NZ.Orz.Routing;

public class RouteTable<T>
{
    private readonly RadixTrie<PriorityRouteDataList<T>> trie;
    private readonly StringComparison comparison;
    private RandomAccessCache<string, PriorityRouteDataList<T>[]> cache;
    private FrozenDictionary<string, PriorityRouteDataList<T>> exact;

    public RouteTable(IDictionary<string, PriorityRouteDataList<T>> exact, RadixTrie<PriorityRouteDataList<T>> trie, int cacheSize, StringComparison comparison)
    {
        cache = new RandomAccessCache<string, PriorityRouteDataList<T>[]>(cacheSize);
        this.trie = trie;
        this.comparison = comparison;
        this.exact = exact.ToFrozenDictionary(MatchComparison(comparison));
    }

    private IEqualityComparer<string>? MatchComparison(StringComparison comparison)
    {
        return comparison switch
        {
            StringComparison.CurrentCulture => StringComparer.CurrentCulture,
            StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
            StringComparison.InvariantCulture => StringComparer.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
            StringComparison.Ordinal => StringComparer.Ordinal,
            StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
        };
    }

    public async ValueTask<T> MatchAsync<R>(string key, R data, Func<T, R, bool> match)
    {
        var all = await FindAllAsync(key);
        if (all == null) return default;
        foreach (var items in all.AsSpan())
        {
            foreach (var item in items)
            {
                var v = item.Value;
                if (match(v, data))
                {
                    return v;
                }
            }
        }
        return default;
    }

    public async ValueTask<T> FirstAsync(string key)
    {
        var all = await FindAllAsync(key);
        if (all == null) return default;
        var f = all.FirstOrDefault();
        if (f == null) return default;
        var f2 = f.FirstOrDefault();
        return f2.Value;
    }

    public async ValueTask<PriorityRouteDataList<T>[]> FindAllAsync(string key)
    {
        if (cache.TryRead(key, out var session))
        {
            return session.Value;
        }
        else
        {
            using var writeSession = await cache.ChangeAsync(key);
            if (!writeSession.TryGetValue(out var value))
            {
                if (exact.TryGetValue(key, out var result))
                {
                    value = [result];
                }
                else
                {
                    value = trie.Search(key, comparison).ToArray();
                    if (value.Length == 0)
                        value = null;
                }
                writeSession.SetValue(value);
            }

            return value;
        }
    }
}