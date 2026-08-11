using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.Liquid;

namespace OrchardCore.Benchmarks;

[MemoryDiagnoser]
public class FluidShapeRenderBenchmark
{
    private static readonly FilterArguments s_filterArguments = new FilterArguments().Add("utc", new DateTimeValue(DateTime.UtcNow)).Add("format", StringValue.Create("MMMM dd, yyyy"));
    private static readonly FluidValue s_input = ObjectValue.Create(HtmlString.Empty, new TemplateOptions());
    private static readonly LiquidFilterDelegateResolver<ShapeRenderFilter> s_liquidFilterDelegateResolver;
    private static readonly IServiceProvider s_serviceProvider;

    static FluidShapeRenderBenchmark()
    {
        var htmlDisplay = new DefaultHtmlDisplay(null, null, null, null, null, Options.Create(new ShapeRenderingOptions()), null);

        s_serviceProvider = new ServiceCollection()
            .AddScoped<IDisplayHelper>(sp => new DisplayHelper(htmlDisplay, null, null))
            .AddTransient<ShapeRenderFilter>()
            .BuildServiceProvider();

        s_liquidFilterDelegateResolver = new LiquidFilterDelegateResolver<ShapeRenderFilter>();
    }

    [Benchmark(Baseline = true)]
#pragma warning disable CA1822 // Mark members as static
    public async Task OriginalShapeRenderDynamic()
    {
        var templateContext = new LiquidTemplateContext(s_serviceProvider, new TemplateOptions());
        var displayHelper = s_serviceProvider.GetRequiredService<IDisplayHelper>();
        templateContext.AmbientValues["DisplayHelper"] = displayHelper;
        await OriginalShapeRenderDynamic(s_input, s_filterArguments, templateContext);
    }

    [Benchmark]
    public async Task ShapeRenderWithAmbientValues()
    {
        var templateContext = new LiquidTemplateContext(s_serviceProvider, new TemplateOptions());
        var displayHelper = s_serviceProvider.GetRequiredService<IDisplayHelper>();
        templateContext.AmbientValues["DisplayHelper"] = displayHelper;
        await ShapeRenderWithAmbientValues(s_input, s_filterArguments, templateContext);
    }

    [Benchmark]
    public async Task ShapeRenderStatic()
    {
        var templateContext = new LiquidTemplateContext(s_serviceProvider, new TemplateOptions());
        await ShapeRenderStatic(s_input, s_filterArguments, templateContext);
    }

    [Benchmark]
    public async Task ShapeRenderWithResolver()
#pragma warning restore CA1822 // Mark members as static
    {
        var templateContext = new LiquidTemplateContext(s_serviceProvider, new TemplateOptions());
        await s_liquidFilterDelegateResolver.ResolveAsync(s_input, s_filterArguments, templateContext);
    }

    private static async ValueTask<FluidValue> OriginalShapeRenderDynamic(FluidValue input, FilterArguments _, TemplateContext context)
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

    private static ValueTask<FluidValue> ShapeRenderWithAmbientValues(FluidValue input, FilterArguments _, TemplateContext context)
    {
        static async ValueTask<FluidValue> Awaited(Task<IHtmlContent> task)
        {
            return new HtmlContentValue(await task);
        }

        if (input.ToObjectValue() is IShape shape)
        {
            if (!context.AmbientValues.TryGetValue("DisplayHelper", out var item) || item is not IDisplayHelper displayHelper)
            {
                return ThrowArgumentException<ValueTask<FluidValue>>("DisplayHelper missing while invoking 'shape_render'");
            }

            var task = displayHelper.ShapeExecuteAsync(shape);
            if (!task.IsCompletedSuccessfully)
            {
                return Awaited(task);
            }
            return ValueTask.FromResult<FluidValue>(new HtmlContentValue(task.Result));
        }

        return ValueTask.FromResult<FluidValue>(NilValue.Instance);
    }

    private static ValueTask<FluidValue> ShapeRenderStatic(FluidValue input, FilterArguments _, TemplateContext context)
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

            return ValueTask.FromResult<FluidValue>(new HtmlContentValue(task.Result));
        }

        return ValueTask.FromResult<FluidValue>(NilValue.Instance);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowArgumentException<T>(string message)
    {
        throw new ArgumentException(message);
    }
}
