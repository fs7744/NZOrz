using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using DotNext.Runtime.Caching;
using NZ.Orz.Routing;
using System.Collections.Concurrent;

namespace NZOrz.Benchmarks;

[MemoryDiagnoser, Orderer(summaryOrderPolicy: SummaryOrderPolicy.FastestToSlowest), GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class RadixTrieBenchmarks
{
    private RadixTrie<List<string>> trie;
    private List<string> data;
    private ConcurrentDictionary<string, List<string>[]> Cache;
    private ConcurrentDictionary<string, List<string>> Cache2;
    private RandomAccessCache<string, List<string>> randomAccessCache;
    private RandomAccessCache<string, List<string>[]> randomAccessCache2;

    [Params(10, 100, 1000, 10000)]
    public int Count { get; set; }

    public string Test { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        randomAccessCache = new RandomAccessCache<string, List<string>>(1000);
        randomAccessCache2 = new RandomAccessCache<string, List<string>[]>(1000);
        trie = new RadixTrie<List<string>>();
        data = new List<string>();
        Func<List<string>, List<string>, List<string>> merge = (i, j) =>
        {
            if (i is null) return j;
            if (j is null) return i;
            i.AddRange(j);
            return i;
        };
        void Add(string d)
        {
            data.Add(d);
            trie.Add(d, new List<string>() { d }, merge);
        }
        var d = GetStrings(10);
        for (int i = 0; i < Count; i++)
        {
            var a = Random.Shared.Next(5);
            var s = "/" + string.Join("/", Enumerable.Repeat(a, a).Select(i => d[Random.Shared.Next(10)]));
            Add(s);
        }
        data = data.OrderByDescending(i => i).ToList();
        Test = data[Random.Shared.Next(data.Count / 5)];
        Cache = new ConcurrentDictionary<string, List<string>[]>();
        Cache2 = new ConcurrentDictionary<string, List<string>>();
    }

    private static string[] GetStrings(int count)
    {
        var strings = new string[count];
        for (var i = 0; i < count; i++)
        {
            var guid = Guid.NewGuid().ToString();

            // Between 5 and 36 characters
            var text = guid.Substring(0, Math.Max(5, Math.Min(i, 36)));

            // Convert first half of text to letters
            text = string.Create(text.Length, text, static (buffer, state) =>
            {
                for (var c = 0; c < buffer.Length; c++)
                {
                    buffer[c] = char.ToUpperInvariant(state[c]);

                    if (char.IsDigit(buffer[c]) && c < buffer.Length / 2)
                    {
                        buffer[c] = ((char)(state[c] + ('G' - '0')));
                    }
                }
            });

            if (i % 2 == 0)
            {
                // Lowercase half of them
                text = text.ToLowerInvariant();
            }

            strings[i] = text;
        }

        return strings;
    }

    [Benchmark(Baseline = true), BenchmarkCategory("First")]
    public string BaselineFirst()
    {
        return data.Where(i => Test.StartsWith(i)).FirstOrDefault();
    }

    [Benchmark, BenchmarkCategory("First")]
    public List<string> RadixTrieFirst()
    {
        return trie.Search(Test).FirstOrDefault();
    }

    [Benchmark, BenchmarkCategory("First")]
    public List<string> RadixTrieFirstCache()
    {
        return Cache2.GetOrAdd(Test, k => trie.Search(k).FirstOrDefault());
    }

    [Benchmark, BenchmarkCategory("First")]
    public List<string> RadixTrieFirstRandomAccessCache()
    {
        if (randomAccessCache.TryRead(Test, out var result))
        {
            return result.Value;
        }
        var a = randomAccessCache.Change(Test, TimeSpan.FromSeconds(10));
        var v = trie.Search(Test).FirstOrDefault();
        a.SetValue(v);
        return v;
    }

    [Benchmark(Baseline = true), BenchmarkCategory("All")]
    public string[] BaselineAll()
    {
        return data.Where(i => Test.StartsWith(i)).ToArray();
    }

    [Benchmark, BenchmarkCategory("All")]
    public List<string>[] RadixTrieAllRandomAccessCache()
    {
        if (randomAccessCache2.TryRead(Test, out var result))
        {
            return result.Value;
        }
        var a = randomAccessCache2.Change(Test, TimeSpan.FromSeconds(10));
        var v = trie.Search(Test).ToArray();
        a.SetValue(v);
        return v;
    }

    [Benchmark, BenchmarkCategory("All")]
    public List<string>[] RadixTrieAll()
    {
        return trie.Search(Test).ToArray();
    }

    [Benchmark, BenchmarkCategory("All")]
    public List<string>[] RadixTrieAllCache()
    {
        return Cache.GetOrAdd(Test, k => trie.Search(k).ToArray());
    }
}