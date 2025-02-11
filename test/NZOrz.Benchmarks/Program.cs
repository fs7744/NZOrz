using BenchmarkDotNet.Running;
using NZ.Orz.Benchmarks;

var a = new ParametersBenchmarks().F2Get();
var summary = BenchmarkRunner.Run<ParametersBenchmarks>();