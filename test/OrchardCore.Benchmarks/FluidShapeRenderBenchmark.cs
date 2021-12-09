using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.Liquid;

namespace OrchardCore.Benchmark
{
    [MemoryDiagnoser]
    public class FluidShapeRenderBenchmark
    {
        private static readonly FilterArguments _filterArguments = new FilterArguments().Add("utc", new DateTimeValue(DateTime.UtcNow)).Add("format", StringValue.Create("MMMM dd, yyyy"));
        private static readonly FluidValue _input = ObjectValue.Create(HtmlString.Empty, new TemplateOptions());
        private static readonly LiquidFilterDelegateResolver<ShapeRenderFilter> _liquidFilterDelegateResolver;
        private static readonly IServiceProvider _serviceProvider;

        static FluidShapeRenderBenchmark()
        {
            var htmlDisplay = new DefaultHtmlDisplay(null, null, null, null, null, null);

            _serviceProvider = new ServiceCollection()
                .AddScoped<IDisplayHelper>(sp => new DisplayHelper(htmlDisplay, null, null))
                .AddTransient(typeof(ShapeRenderFilter))
                .BuildServiceProvider();

            _liquidFilterDelegateResolver = new LiquidFilterDelegateResolver<ShapeRenderFilter>();
        }

        // Summary 19th May 2021: dotnet run -c Release --filter *FluidShapeRenderBenchmark* --framework netcoreapp3.1 --job short

        //BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.985 (21H1/May2021Update)
        //Intel Core i9-9980HK CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
        //.NET SDK= 6.0.100-preview.3.21202.5
        //  [Host]   : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT
        //  ShortRun : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT

        //Job=ShortRun IterationCount = 3  LaunchCount=1
        //WarmupCount=3

        //|                       Method |     Mean |    Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
        //|----------------------------- |---------:|---------:|----------:|------:|--------:|-------:|-------:|------:|----------:|
        //|   OriginalShapeRenderDynamic | 6.211 us | 5.331 us | 0.2922 us |  1.00 |    0.00 | 1.1292 | 0.0153 |     - |      9 KB |
        //| ShapeRenderWithAmbientValues | 5.441 us | 3.280 us | 0.1798 us |  0.88 |    0.04 | 1.0910 | 0.0229 |     - |      9 KB |
        //|            ShapeRenderStatic | 4.498 us | 3.363 us | 0.1844 us |  0.72 |    0.02 | 1.0757 | 0.0229 |     - |      9 KB |
        //|      ShapeRenderWithResolver | 4.442 us | 5.901 us | 0.3235 us |  0.71 |    0.03 | 1.0834 | 0.0229 |     - |      9 KB |


        // Summary 19th May 2021: dotnet run -c Release --filter *FluidShapeRenderBenchmark* --framework net5.0 --job short

        //BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.985 (21H1/May2021Update)
        //Intel Core i9-9980HK CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
        //.NET SDK= 6.0.100-preview.3.21202.5
        //  [Host]   : .NET 5.0.6 (5.0.621.22011), X64 RyuJIT
        //  ShortRun : .NET 5.0.6 (5.0.621.22011), X64 RyuJIT

        //Job=ShortRun IterationCount = 3  LaunchCount=1
        //WarmupCount=3

        //|                       Method |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
        //|----------------------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|------:|----------:|
        //|   OriginalShapeRenderDynamic | 5.391 us | 4.1445 us | 0.2272 us |  1.00 |    0.00 | 1.1444 | 0.0153 |     - |      9 KB |
        //| ShapeRenderWithAmbientValues | 4.954 us | 2.2463 us | 0.1231 us |  0.92 |    0.02 | 1.0986 |      - |     - |      9 KB |
        //|            ShapeRenderStatic | 3.999 us | 2.2435 us | 0.1230 us |  0.74 |    0.03 | 1.0834 | 0.0153 |     - |      9 KB |
        //|      ShapeRenderWithResolver | 3.701 us | 0.2977 us | 0.0163 us |  0.69 |    0.03 | 1.0910 | 0.0229 |     - |      9 KB |

        [Benchmark(Baseline = true)]
        public async Task OriginalShapeRenderDynamic()
        {
            var templateContext = new LiquidTemplateContext(_serviceProvider, new TemplateOptions());
            var displayHelper = _serviceProvider.GetRequiredService<IDisplayHelper>();
            templateContext.AmbientValues["DisplayHelper"] = displayHelper;
            await OriginalShapeRenderDynamic(_input, _filterArguments, templateContext);
        }

        [Benchmark]
        public async Task ShapeRenderWithAmbientValues()
        {
            var templateContext = new LiquidTemplateContext(_serviceProvider, new TemplateOptions());
            var displayHelper = _serviceProvider.GetRequiredService<IDisplayHelper>();
            templateContext.AmbientValues["DisplayHelper"] = displayHelper;
            await ShapeRenderWithAmbientValues(_input, _filterArguments, templateContext);
        }

        [Benchmark]
        public async Task ShapeRenderStatic()
        {
            var templateContext = new LiquidTemplateContext(_serviceProvider, new TemplateOptions());
            await ShapeRenderStatic(_input, _filterArguments, templateContext);
        }

        [Benchmark]
        public async Task ShapeRenderWithResolver()
        {
            var templateContext = new LiquidTemplateContext(_serviceProvider, new TemplateOptions());
            await _liquidFilterDelegateResolver.ResolveAsync(_input, _filterArguments, templateContext);
        }

        private static async ValueTask<FluidValue> OriginalShapeRenderDynamic(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'shape_render'");
            }

            // This is marginally different than the exact original as we currently pass any non null object to the display helper.
            // And the original benchmark was if (input.ToObjectValue() is IShape shape) where input was never IShape.
            // The original benchmark noop'd here and didn't hit the dynamic display helper.
            if (input != null)
            {
                return new HtmlContentValue(await (Task<IHtmlContent>)displayHelper(input));
            }

            return NilValue.Instance;
        }

        private static ValueTask<FluidValue> ShapeRenderWithAmbientValues(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            static async ValueTask<FluidValue> Awaited(Task<IHtmlContent> task)
            {
                return new HtmlContentValue(await task);
            }

            if (input.ToObjectValue() is IShape shape)
            {
                if (!context.AmbientValues.TryGetValue("DisplayHelper", out var item) || !(item is IDisplayHelper displayHelper))
                {
                    return ThrowArgumentException<ValueTask<FluidValue>>("DisplayHelper missing while invoking 'shape_render'");
                }

                var task = displayHelper.ShapeExecuteAsync(shape);
                if (!task.IsCompletedSuccessfully)
                {
                    return Awaited(task);
                }
                return new ValueTask<FluidValue>(new HtmlContentValue(task.Result));
            }

            return new ValueTask<FluidValue>(NilValue.Instance);
        }

        private static ValueTask<FluidValue> ShapeRenderStatic(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            static async ValueTask<FluidValue> Awaited(Task<IHtmlContent> task)
            {
                return new HtmlContentValue(await task);
            }

            if (input.ToObjectValue() is IShape shape)
            {
                var services = ((LiquidTemplateContext)context).Services;

                var displayHelper = services.GetRequiredService<IDisplayHelper>();

                var task = displayHelper.ShapeExecuteAsync(shape);
                if (!task.IsCompletedSuccessfully)
                {
                    return Awaited(task);
                }
                return new ValueTask<FluidValue>(new HtmlContentValue(task.Result));
            }

            return new ValueTask<FluidValue>(NilValue.Instance);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T ThrowArgumentException<T>(string message)
        {
            throw new ArgumentException(message);
        }
    }
}
