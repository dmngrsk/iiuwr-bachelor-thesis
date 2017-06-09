using BenchmarkDotNet.Running;

namespace Thesis.Relinq.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Reports are generated in the /bin/[Platform]/BenchmarkDotNet.Artifacts/results folder.

            BenchmarkRunner.Run<DevartLinqConnectBenchmark>();
            BenchmarkRunner.Run<LinqToDbBenchmark>();
            BenchmarkRunner.Run<ThesisBenchmark>();
        }
    }
}