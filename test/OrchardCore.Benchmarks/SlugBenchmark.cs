using BenchmarkDotNet.Attributes;
using OrchardCore.Modules.Services;

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
         * BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1415 (20H2/October2020Update)
         * Intel Core i7-3687U CPU 2.10GHz(Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
         * .NET SDK= 6.0.100
         *
         * [Host]   : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
         * ShortRun : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
         *
         * Job=ShortRun IterationCount = 3  LaunchCount=1
         * WarmupCount=3
         * 
         * |          Method |     Mean |     Error |    StdDev |  Gen 0 | Allocated |
         * |---------------- |---------:|----------:|----------:|-------:|----------:|
         * | EvaluateSlugify | 1.477 us | 0.5187 us | 0.0284 us | 0.2174 |     456 B |
         */

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
}
