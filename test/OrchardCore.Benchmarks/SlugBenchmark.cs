using BenchmarkDotNet.Attributes;
using OrchardCore.Liquid;
using OrchardCore.Liquid.Services;

namespace OrchardCore.Benchmark
{
    [MemoryDiagnoser]
    public class SlugBenchmark
    {
        private static readonly SlugService _slugService;

        static SlugBenchmark()
        {
            _slugService = new SlugService();
        }

        /*
         * Summary 24th December 2021
         *
         * BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2006/21H2/November2021Update)
         * Intel Core i7-3687U CPU 2.10GHz(Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
         * .NET SDK= 6.0.301
         *
         * [Host]     : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT AVX
         * DefaultJob : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT AVX
         *
         * Job=ShortRun IterationCount = 3  LaunchCount=1
         * WarmupCount=3
         * 
         * |                       Method |        Mean |     Error |    StdDev |   Gen0 | Allocated |
         * |----------------------------- |------------:|----------:|----------:|-------:|----------:|
         * | EvaluateSlugifyWithShortSlug |    782.3 ns |   5.05 ns |   4.73 ns | 0.0801 |     168 B |
         * |  EvaluateSlugifyWithLongSlug | 11,815.4 ns | 224.44 ns | 249.46 ns | 1.2054 |    2528 B |
         */

        [Benchmark]
        public void EvaluateSlugifyWithShortSlug()
        {
            _slugService.Slugify("Je veux aller à Saint-Étienne");
        }

	    [Benchmark]
        public void EvaluateSlugifyWithLongSlug()
        {
            _slugService.Slugify("Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne Je veux aller à Saint-Étienne");
        }
    }
}
