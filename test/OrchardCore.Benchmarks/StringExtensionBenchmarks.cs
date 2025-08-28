using BenchmarkDotNet.Attributes;
using OrchardCore.Modules;

namespace OrchardCore.Benchmarks;

[MemoryDiagnoser]
public class StringExtensionBenchmarks
{
    private const string _sample = "Hello\r\nWorld\n. This is a test\rstring\nwith multiple\r\nline breaks\n\nand some extra\r\rcharacters.";

#pragma warning disable CA1822 // Mark members as static
    [Benchmark]
    public string UsingDoubleReplace()
    {
        var value = _sample.Replace("\r", string.Empty).Replace("\n", string.Empty);

        return value;
    }

    [Benchmark]
    public string UsingRemoveLineBreaks()
    {
        var value = _sample.RemoveLineBreaks();

        return value;
    }
#pragma warning restore CA1822 // Mark members as static
}
