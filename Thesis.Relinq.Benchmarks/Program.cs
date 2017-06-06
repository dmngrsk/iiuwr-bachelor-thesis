using BenchmarkDotNet.Running;
using System;

namespace Thesis.Relinq.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PsqlQueryableBenchmark>();
        }
    }
}