using BenchmarkDotNet.Running;

namespace OrchardCore.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(ShapeFactoryBenchmark).Assembly).Run(args);
        }
    }
}
