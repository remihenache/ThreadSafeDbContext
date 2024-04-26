using BenchmarkDotNet.Running;

namespace ThreadSafeDbContext.Benchmark;

internal class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<DbContextBenchmarker>();
    }
}