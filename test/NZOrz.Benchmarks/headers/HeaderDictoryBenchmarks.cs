using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.Primitives;
using NZ.Orz.Http;
using NZOrz.Benchmarks.headers;

namespace NZOrz.Benchmarks;

[MemoryDiagnoser, Orderer(summaryOrderPolicy: SummaryOrderPolicy.FastestToSlowest), GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class HeaderDictoryBenchmarks
{
    private HttpRequestHeaders headers = new HttpRequestHeaders();
    private IEnumerator<KeyValuePair<string, StringValues>> headersEnumerator;
    private Dictionary<string, StringValues> dict = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
    private StringValuesHeaderDict sdict = new();
    private StringValuesHeaderDict2 sdict2 = new();

    private void set(string key, StringValues v)
    {
        headers[key] = v;
        dict[key] = v;
        sdict[key] = v.ToString();
        sdict2[key] = v;
    }

    [GlobalSetup]
    public void Setup()
    {
        headersEnumerator = headers.GetEnumerator();

        set("Host", "Host");
        set("User-Agent", "Host");
        set("Accept", "Host");
        set("Accept-Encoding", "Host");
        set("Connection", "Host");
        set(TestHeaderNames.ContentLength, "333");
    }

    //[IterationCleanup]
    //public void Clean()
    //{
    //    set("Host", "Host");
    //    set("AcceptRanges", "AcceptRanges");
    //}

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
        if (dict[TestHeaderNames.ContentLength] != headers.ContentLength.GetValueOrDefault().ToString()) throw new InvalidOperationException();
    }

    [Benchmark, BenchmarkCategory("GetSet")]
    public StringValues GetSet()
    {
        dict[TestHeaderNames.Host] = "3";
        return dict[TestHeaderNames.Host];
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
        headers[TestHeaderNames.Host] = "3";
        return headers[TestHeaderNames.Host];
    }

    [Benchmark, BenchmarkCategory("GetSet")]
    public StringV StringValuesHeaderDict()
    {
        sdict[TestHeaderNames.Host] = "3";
        return sdict[TestHeaderNames.Host];
    }

    [Benchmark, BenchmarkCategory("GetSet")]
    public StringValues HttpRequestHeadersDict2()
    {
        sdict2[TestHeaderNames.Host] = "3";
        return sdict2[TestHeaderNames.Host];
    }

    [Benchmark, BenchmarkCategory("GetSet")]
    public StringValues HttpRequestHeadersUnknownDict()
    {
        headers[TestHeaderNames.AcceptRanges] = "3";
        return headers[TestHeaderNames.AcceptRanges];
    }

    [Benchmark, BenchmarkCategory("GetSet")]
    public StringV StringValuesHeaderDictAppend()
    {
        sdict[TestHeaderNames.AcceptRanges] = "3";
        sdict.Append(TestHeaderNames.AcceptRanges, "3");
        return sdict[TestHeaderNames.AcceptRanges];
    }

    [Benchmark, BenchmarkCategory("GetSet")]
    public StringValues HttpRequestHeadersDict2Append()
    {
        sdict2[TestHeaderNames.AcceptRanges] = "3";
        sdict2.Append(TestHeaderNames.AcceptRanges, "3");
        return sdict2[TestHeaderNames.AcceptRanges];
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