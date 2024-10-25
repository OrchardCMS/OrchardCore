using BenchmarkDotNet.Attributes;
using OrchardCore.Modules.Services;

namespace OrchardCore.Benchmark;

[MemoryDiagnoser]
public class SlugBenchmark
{
    private static readonly SlugService _slugService;

    static SlugBenchmark()
    {
        _slugService = new SlugService();
    }

    [Benchmark]
#pragma warning disable CA1822 // Mark members as static
    public void EvaluateSlugifyWithShortSlug()
    {
        _slugService.Slugify("Je veux aller à Saint-Étienne");
    }

    [Benchmark]
    public void EvaluateSlugifyWithLongSlug()
    {
        _slugService.Slugify("Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne");
    }
#pragma warning restore CA1822 // Mark members as static
}
