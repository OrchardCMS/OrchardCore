using BenchmarkDotNet.Running;
using System;
using System.Linq;

namespace OrchardCore.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Any(arg => arg.Contains("dotnet-test-pipe", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(ShapeFactoryBenchmark).Assembly).Run(args);
    }
}
