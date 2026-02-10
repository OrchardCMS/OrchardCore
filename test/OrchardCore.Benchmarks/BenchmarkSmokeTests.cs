using System.Linq;
using BenchmarkDotNet.Attributes;
using Xunit;

namespace OrchardCore.Benchmarks;

public class BenchmarkSmokeTests
{
    [Fact]
    public void BenchmarksProject_ShouldLoad()
    {
        var benchmarkMethods = typeof(ShapeFactoryBenchmark).Assembly
            .GetTypes()
            .SelectMany(type => type.GetMethods())
            .Where(method => method.GetCustomAttributes(typeof(BenchmarkAttribute), inherit: true).Length > 0)
            .ToArray();

        Assert.NotEmpty(benchmarkMethods);
    }
}
