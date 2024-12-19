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

namespace OrchardCore.Benchmark;

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
            .AddTransient<ShapeRenderFilter>()
            .BuildServiceProvider();

        _liquidFilterDelegateResolver = new LiquidFilterDelegateResolver<ShapeRenderFilter>();
    }

    [Benchmark(Baseline = true)]
#pragma warning disable CA1822 // Mark members as static
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
#pragma warning restore CA1822 // Mark members as static
    {
        var templateContext = new LiquidTemplateContext(_serviceProvider, new TemplateOptions());
        await _liquidFilterDelegateResolver.ResolveAsync(_input, _filterArguments, templateContext);
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
