using BenchmarkDotNet.Running;
using NZOrz.Benchmarks;

new TypeMapBenchmarks().Setup();
new TypeMapBenchmarks().TypeMapLookupSetPool();
var summary = BenchmarkRunner.Run<TypeMapBenchmarks>();