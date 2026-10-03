using BenchmarkDotNet.Attributes;
using OrchardCore.Modules.Services;

namespace OrchardCore.Benchmarks;

[MemoryDiagnoser]
public class SlugBenchmark
{
    private static readonly SlugService s_slugService;

    static SlugBenchmark()
    {
        s_slugService = new SlugService();
    }

    [Benchmark]
#pragma warning disable CA1822 // Mark members as static
    public void EvaluateSlugifyWithShortSlug()
    {
        s_slugService.Slugify("Je veux aller à Saint-Étienne");
    }

    [Benchmark]
    public void EvaluateSlugifyWithLongSlug()
    {
        s_slugService.Slugify("Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne");
    }
#pragma warning restore CA1822 // Mark members as static
}
