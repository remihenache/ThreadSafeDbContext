using BenchmarkDotNet.Running;

namespace ThreadSafeDbContext.Benchmark;

internal class Program
{
    private static void Main(String[] args)
    {
        BenchmarkRunner.Run<DbContextBenchmarker>();
    }
}