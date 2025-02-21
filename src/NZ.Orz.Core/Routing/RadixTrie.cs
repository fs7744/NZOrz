namespace NZ.Orz.Routing;

public class RadixTrieNode<T>
{
    public string Key;
    public T? Value;
    public List<RadixTrieNode<T>> Children;
}

public class RadixTrie<T>
{
    private readonly RadixTrieNode<T> trie;

    public RadixTrie()
    {
        trie = new RadixTrieNode<T>();
    }

    public void Add(string key, T value, Func<T?, T?, T?> merge)
    {
        Add(trie, key, value, merge);
    }

    public static void Add(RadixTrieNode<T> curr, string term, T value, Func<T?, T?, T?> merge)
    {
        int common = 0;
        if (curr.Children != null)
        {
            for (int j = 0; j < curr.Children.Count; j++)
            {
                var node = curr.Children[j];
                var key = node.Key;
                for (int i = 0; i < Math.Min(term.Length, key.Length); i++) if (term[i] == key[i]) common = i + 1; else break;

                if (common > 0)
                {
                    //term already existed
                    //existing ab
                    //new      ab
                    if ((common == term.Length) && (common == key.Length))
                    {
                        node.Value = merge(node.Value, value);
                    }//new is subkey
                     //existing abcd
                     //new      ab
                     //if new is shorter (== common), then node(count) and only 1. children add (clause2)
                    else if (common == term.Length)
                    {
                        node.Key = key.Substring(common);
                        var child = new RadixTrieNode<T>() { Key = term.Substring(0, common), Children = new List<RadixTrieNode<T>>() { node } };
                        child.Value = value;
                        curr.Children[j] = child;
                    }
                    //if oldkey shorter (==common), then recursive addTerm (clause1)
                    //existing: te
                    //new:      test
                    else if (common == key.Length)
                    {
                        Add(node, term.Substring(common), value, merge);
                    }
                    //old and new have common substrings
                    //existing: test
                    //new:      team
                    else
                    {
                        var child = new RadixTrieNode<T>() { Key = term.Substring(0, common) };
                        node.Key = key.Substring(common);
                        child.Children = new List<RadixTrieNode<T>>() { node, new RadixTrieNode<T>() { Key = term.Substring(common), Value = value } };
                        curr.Children[j] = child;
                    }
                    return;
                }
            }
        }

        if (curr.Children == null)
        {
            curr.Children = new List<RadixTrieNode<T>>() { new RadixTrieNode<T>() { Key = term, Value = value } };
        }
        else
        {
            curr.Children.Add(new RadixTrieNode<T>() { Key = term, Value = value });
        }
    }

    public IEnumerable<T> Search(string key, StringComparison comparison = StringComparison.Ordinal)
    {
        return Search(trie, new StringSegment(0, key), comparison);
    }

    private static IEnumerable<T> Search(RadixTrieNode<T> trie, StringSegment key, StringComparison comparison)
    {
        if (trie.Children == null) yield break;

        foreach (var item in trie.Children)
        {
            if (key.GetSpan.StartsWith(item.Key, comparison))
            {
                if (trie.Children != null)
                {
                    foreach (var item1 in Search(item, new StringSegment(key.Start + item.Key.Length, key.String), comparison))
                    {
                        yield return item1;
                    }
                }

                if (item.Value != null)
                {
                    yield return item.Value;
                }
            }
        }
    }
}

internal readonly struct StringSegment
{
    public readonly int Start;
    public readonly string String;

    public StringSegment(int start, string str)
    {
        Start = start;
        String = str;
    }

    public ReadOnlySpan<char> GetSpan => String.AsSpan(Start);
}