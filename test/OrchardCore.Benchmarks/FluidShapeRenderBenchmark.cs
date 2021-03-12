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

        // 21st Feb 2021

        // BenchmarkDotNet=v0.12.1, OS=macOS Catalina 10.15.7 (19H15) [Darwin 19.6.0]
        // Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
        // .NET Core SDK=5.0.103
        //   [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
        //   DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT


        // |                       Method |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
        // |----------------------------- |---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
        // |   OriginalShapeRenderDynamic | 3.798 us | 0.0663 us | 0.1108 us |  1.00 |    0.00 | 2.2888 |     - |     - |   9.37 KB |
        // | ShapeRenderWithAmbientValues | 3.156 us | 0.0278 us | 0.0232 us |  0.82 |    0.03 | 2.2240 |     - |     - |   9.09 KB |
        // |            ShapeRenderStatic | 3.180 us | 0.0253 us | 0.0224 us |  0.83 |    0.03 | 2.1935 |     - |     - |   8.96 KB |
        // |      ShapeRenderWithResolver | 3.164 us | 0.0206 us | 0.0161 us |  0.83 |    0.03 | 2.1973 |     - |     - |   8.98 KB |

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
