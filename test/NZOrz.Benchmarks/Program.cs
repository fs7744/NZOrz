using BenchmarkDotNet.Running;
using NZ.Orz.Benchmarks;
using NZOrz.Benchmarks;
using System.Numerics;

new HeaderDictoryBenchmarks().Test();
var summary = BenchmarkRunner.Run<HeaderDictoryBenchmarks>();

//test aa = test.Accept | test.AcceptLanguage;
//var cc = 0b_1;
//var ccc = 0b_10;
//var cccc = 0b_100;
//var ccccc = 0b_1000;
//var bb = (ulong)aa;
//var _next1 = BitOperations.PopCount(bb);
//var _next = BitOperations.TrailingZeroCount(bb);
//bb ^= (ulong)test.Accept;
//_next = BitOperations.TrailingZeroCount(bb);
//bb ^= (ulong)test.AcceptLanguage;
//_next = BitOperations.TrailingZeroCount(bb);
//_next = BitOperations.TrailingZeroCount(bb);
//_next = BitOperations.TrailingZeroCount(bb);

//;