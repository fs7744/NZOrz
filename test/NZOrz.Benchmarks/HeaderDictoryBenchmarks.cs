using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.Primitives;
using NZ.Orz.Http;

namespace NZOrz.Benchmarks;

[MemoryDiagnoser, Orderer(summaryOrderPolicy: SummaryOrderPolicy.FastestToSlowest), GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class HeaderDictoryBenchmarks
{
    private HttpRequestHeaders headers = new HttpRequestHeaders();
    private IEnumerator<KeyValuePair<string, StringValues>> headersEnumerator;
    private Dictionary<string, StringValues> dict = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

    [GlobalSetup]
    public void Setup()
    {
        headersEnumerator = headers.GetEnumerator();
        void set(string key, StringValues v)
        {
            headers[key] = v;
            dict[key] = v;
        }

        set("Host", "Host");
        //set("User-Agent", "Host");
        //set("Accept", "Host");
        //set("Accept-Encoding", "Host");
        //set("Connection", "Host");
    }

    public void Test()
    {
        Setup();
        var a = GetSet();
        var b = HttpRequestHeadersGetSet();
        if (a != b) throw new InvalidOperationException();
        var c = HttpRequestHeadersDict();
        if (a != c) throw new InvalidOperationException();

        var ac = Count();
        var bc = HttpRequestHeadersCount();
        if (ac != bc) throw new InvalidOperationException();

        var ae = ArraryCopyTo();
        var be = HttpRequestHeadersArraryCopyTo();
        if (ae.Length != be.Length) throw new InvalidOperationException();

        ae = Enumerator();
        be = HttpRequestHeadersEnumerator();
        if (ae.Length != be.Length) throw new InvalidOperationException();
    }

    [Benchmark, BenchmarkCategory("GetSet")]
    public StringValues GetSet()
    {
        dict[HeaderNames.Host] = "3";
        return dict[HeaderNames.Host];
    }

    [Benchmark, BenchmarkCategory("GetSet")]
    public StringValues HttpRequestHeadersGetSet()
    {
        headers.Host = "3";
        return headers.Host;
    }

    [Benchmark, BenchmarkCategory("GetSet")]
    public StringValues HttpRequestHeadersDict()
    {
        headers[HeaderNames.Host] = "3";
        return headers[HeaderNames.Host];
    }

    [Benchmark, BenchmarkCategory("Count")]
    public int Count()
    {
        return dict.Count;
    }

    [Benchmark, BenchmarkCategory("Count")]
    public int HttpRequestHeadersCount()
    {
        return headers.Count;
    }

    [Benchmark, BenchmarkCategory("Enumerator")]
    public KeyValuePair<string, StringValues>[] Enumerator()
    {
        List<KeyValuePair<string, StringValues>> list = new List<KeyValuePair<string, StringValues>>();
        foreach (var item in dict)
        {
            list.Add(item);
        }
        return list.ToArray();
    }

    [Benchmark, BenchmarkCategory("Enumerator")]
    public KeyValuePair<string, StringValues>[] HttpRequestHeadersEnumerator()
    {
        List<KeyValuePair<string, StringValues>> list = new List<KeyValuePair<string, StringValues>>();
        var headersEnumerator = headers.GetEnumerator();
        while (headersEnumerator.MoveNext())
        {
            list.Add(headersEnumerator.Current);
        }
        return list.ToArray();
    }

    [Benchmark, BenchmarkCategory("Enumerator")]
    public KeyValuePair<string, StringValues>[] HttpRequestHeadersEnumeratorCache()
    {
        List<KeyValuePair<string, StringValues>> list = new List<KeyValuePair<string, StringValues>>();
        headersEnumerator.Reset();
        while (headersEnumerator.MoveNext())
        {
            list.Add(headersEnumerator.Current);
        }
        return list.ToArray();
    }

    [Benchmark, BenchmarkCategory("ArraryCopyTo")]
    public KeyValuePair<string, StringValues>[] ArraryCopyTo()
    {
        return dict.ToArray();
    }

    [Benchmark, BenchmarkCategory("ArraryCopyTo")]
    public KeyValuePair<string, StringValues>[] HttpRequestHeadersArraryCopyTo()
    {
        return headers.ToArray();
    }
}