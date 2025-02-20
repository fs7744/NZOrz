using BenchmarkDotNet.Running;
using NZ.Orz.Benchmarks;
using NZOrz.Benchmarks;

var a = new ParametersBenchmarks().F2Get();
var summary = BenchmarkRunner.Run<RadixTrieBenchmarks>();