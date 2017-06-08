using BenchmarkDotNet.Running;

namespace Thesis.Relinq.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<DotConnectBenchmark>();
        }
    }
}