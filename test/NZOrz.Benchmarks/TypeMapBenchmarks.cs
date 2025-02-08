using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using CommandLine;
using DotNext.Collections.Specialized;
using DotNext.Reflection;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Reflection;

namespace NZOrz.Benchmarks
{
    [ShortRunJob, MemoryDiagnoser, Orderer(summaryOrderPolicy: SummaryOrderPolicy.FastestToSlowest), GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
    public class TypeMapBenchmarks
    {
        private sealed class DictionaryBasedLookup<TValue> : Dictionary<Type, TValue>
        {
            public void Set<TKey>(TValue value) => this[typeof(TKey)] = value;

            public bool TryGetValue<TKey>(out TValue value) => TryGetValue(typeof(TKey), out value);
        }

        private sealed class ConcurrentDictionaryBasedLookup<TValue> : ConcurrentDictionary<Type, TValue>
        {
            public void Set<TKey>(TValue value) => this[typeof(TKey)] = value;

            public bool TryGetValue<TKey>(out TValue value) => TryGetValue(typeof(TKey), out value);

            public TValue GetOrAdd<TKey>(TValue value) => GetOrAdd(typeof(TKey), value);
        }

        private TypeMap<int> threadUnsafeMap = new();
        private ConcurrentTypeMap<int> threadSafeMap = new();
        private DictionaryBasedLookup<int> dictionaryLookup = new();
        private ConcurrentDictionaryBasedLookup<int> concurrentLookup = new();
        private ObjectPool<TypeMap<int>> pool = ObjectPool.Create<TypeMap<int>>(new TypeMapPolicy());

        private class TypeMapPolicy : IPooledObjectPolicy<TypeMap<int>>
        {
            public TypeMap<int> Create()
            {
                return new();
            }

            public bool Return(TypeMap<int> obj)
            {
                obj.Clear();
                return true;
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            var ts = AppDomain.CurrentDomain.GetAssemblies().SelectMany(i => i.GetExportedTypes()).Where(i => !i.IsGenericType && i.IsClass).Where(i => !i.FullName.Equals("System.String", StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var item in ts)
            {
                threadUnsafeMap.GetType().GetMembers().First(i => i.Name.StartsWith("Set")).Cast<MethodInfo>().MakeGenericMethod(item).Invoke(threadUnsafeMap, new object[] { 2 });
                dictionaryLookup.GetType().GetMembers().First(i => i.Name.StartsWith("Set")).Cast<MethodInfo>().MakeGenericMethod(item).Invoke(dictionaryLookup, new object[] { 2 });
                threadSafeMap.GetType().GetMembers().First(i => i.Name.StartsWith("Set")).Cast<MethodInfo>().MakeGenericMethod(item).Invoke(threadSafeMap, new object[] { 2 });
                concurrentLookup.GetType().GetMembers().First(i => i.Name.StartsWith("Set")).Cast<MethodInfo>().MakeGenericMethod(item).Invoke(concurrentLookup, new object[] { 2 });
            }
            threadUnsafeMap.Set<string>(42);
            dictionaryLookup.Set<string>(42);
            threadSafeMap.Set<string>(42);
            concurrentLookup.Set<string>(42);
        }

        [Benchmark(Description = "TypeMap, Set"), BenchmarkCategory("Set")]
        public void TypeMapLookupSet()
        {
            threadUnsafeMap = new();
            threadUnsafeMap.Set<string>(42);
        }

        [Benchmark(Description = "TypeMap, SetPool"), BenchmarkCategory("Set")]
        public void TypeMapLookupSetPool()
        {
            threadUnsafeMap = pool.Get();
            threadUnsafeMap.Set<string>(42);
            pool.Return(threadUnsafeMap);
        }

        [Benchmark(Description = "TypeMap, TryGetValue"), BenchmarkCategory("Get")]
        public int TypeMapLookup()
        {
            threadUnsafeMap.TryGetValue<string>(out var result);
            return result;
        }

        [Benchmark(Description = "Dictionary, Set"), BenchmarkCategory("Set")]
        public void DictionaryLookupSET()
        {
            dictionaryLookup = new();
            dictionaryLookup.Set<string>(42);
        }

        [Benchmark(Description = "Dictionary, TryGetValue"), BenchmarkCategory("Get")]
        public int DictionaryLookup()
        {
            dictionaryLookup.TryGetValue<string>(out var result);
            return result;
        }

        [Benchmark(Description = "ConcurrentTypeMap, Set"), BenchmarkCategory("Set")]
        public void ConcurrentTypeMapLookupSet()
        {
            threadSafeMap = new();
            threadSafeMap.Set<string>(42);
        }

        [Benchmark(Description = "ConcurrentTypeMap, TryGetValue"), BenchmarkCategory("Get")]
        public int ConcurrentTypeMapLookup()
        {
            threadSafeMap.TryGetValue<string>(out var result);
            return result;
        }

        [Benchmark(Description = "ConcurrentDictionary, Set"), BenchmarkCategory("Set")]
        public void ConcurrentDictionaryLookupSet()
        {
            concurrentLookup = new();
            concurrentLookup.Set<string>(42);
        }

        [Benchmark(Description = "ConcurrentDictionary, TryGetValue"), BenchmarkCategory("Get")]
        public int ConcurrentDictionaryLookup()
        {
            concurrentLookup.TryGetValue<string>(out var result);
            return result;
        }

        [Benchmark(Description = "ConcurrentTypeMap, GetOrAdd")]
        public int ConcurrentTypeMapAtomicLookup() => threadSafeMap.GetOrAdd<object>(42, out _);

        [Benchmark(Description = "ConcurrentDictionary, GetOrAdd")]
        public int ConcurrentDictionaryAtomicLookup() => concurrentLookup.GetOrAdd<object>(42);
    }
}