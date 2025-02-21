using DotNext.Runtime.Caching;
using System.Collections.Frozen;

namespace NZ.Orz.Routing;

public class RouteTable<T>
{
    private readonly RadixTrie<T> trie;
    private readonly StringComparison comparison;
    private RandomAccessCache<string, T[]> cache;
    private FrozenDictionary<string, T> exact;

    public RouteTable(IDictionary<string, T> exact, RadixTrie<T> trie, int cacheSize = 1024, StringComparison comparison = StringComparison.Ordinal)
    {
        cache = new RandomAccessCache<string, T[]>(cacheSize);
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

    public async ValueTask<T> FindAsync(string key)
    {
        if (exact.TryGetValue(key, out T result))
        {
            return result;
        }
        if (cache.TryRead(key, out var session))
        {
            return session.Value.FirstOrDefault();
        }
        else
        {
            using var writeSession = await cache.ChangeAsync(key);
            if (!writeSession.TryGetValue(out var value))
            {
                value = trie.Search(key, comparison).ToArray();
                writeSession.SetValue(value);
            }

            return value.FirstOrDefault();
        }
    }

    public async ValueTask<T[]> FindAllAsync(string key)
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
                if (exact.TryGetValue(key, out T result))
                {
                    value = [result];
                }
                else
                {
                    value = trie.Search(key, comparison).ToArray();
                }
                writeSession.SetValue(value);
            }

            return value;
        }
    }
}