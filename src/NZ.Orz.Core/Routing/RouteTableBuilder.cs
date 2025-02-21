using DotNext.Collections.Generic;
using System.Collections.Frozen;

namespace NZ.Orz.Routing;

public class RouteTableBuilder<T>
{
    private readonly RadixTrie<PriorityRouteDataList<T>> trie;
    private readonly StringComparison comparison;
    private readonly int cacheSize;
    private Dictionary<string, PriorityRouteDataList<T>> exact;

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

    public RouteTableBuilder(StringComparison comparison = StringComparison.Ordinal, int cacheSize = 1024)
    {
        this.comparison = comparison;
        this.cacheSize = cacheSize;
        exact = new Dictionary<string, PriorityRouteDataList<T>>(MatchComparison(comparison));
        trie = new RadixTrie<PriorityRouteDataList<T>>();
    }

    public void Add(string key, T value, RouteType type, int priority = 0)
    {
        switch (type)
        {
            case RouteType.Exact:
                var list = exact.GetOrAdd(key, CreatePriorityRouteDataList);
                list.Add(priority, value);
                break;

            case RouteType.Prefix:
                trie.Add(key, new PriorityRouteDataList<T>() { { priority, value } }, MergePriorityRouteDataList);
                break;
        }
    }

    private PriorityRouteDataList<T>? MergePriorityRouteDataList(PriorityRouteDataList<T>? list1, PriorityRouteDataList<T>? list2)
    {
        if (list1 == null) return list2;
        if (list2 == null) return list1;
        list1.AddAll(list2);
        return list1;
    }

    private PriorityRouteDataList<T> CreatePriorityRouteDataList(string arg)
    {
        return new PriorityRouteDataList<T>();
    }

    public RouteTable<T> Build()
    {
        return new RouteTable<T>(exact.ToFrozenDictionary(MatchComparison(comparison)), trie, cacheSize, comparison);
    }
}
