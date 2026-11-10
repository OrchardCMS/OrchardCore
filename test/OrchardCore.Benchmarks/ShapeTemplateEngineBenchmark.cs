using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Benchmarks.Support;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Title.ViewModels;

namespace OrchardCore.Benchmarks;

/// <summary>
/// Renders one shape through each template engine inside a real tenant (the Blog recipe), with
/// <see cref="IShapeHtmlRenderer"/>, so that the measures include what a page pays per shape for the engine
/// itself: the MVC partial view machinery for Razor, the Fluid template execution for Liquid, and only the
/// shape pipeline for a code binding.
/// </summary>
[MemoryDiagnoser]
public class ShapeTemplateEngineBenchmark
{
    private const int OperationsPerInvoke = 100;

    private BlogContext _context;

    [GlobalSetup]
    public async Task SetupAsync()
    {
        _context = new BlogContext();
        await _context.InitializeAsync();

        // Warm up: builds the shape table of the theme, compiles the first Razor view lookups, parses the Liquid template.
        await RenderAsync(CreatePreRenderedShape);
        await RenderAsync(CreateCodeShape);
        await RenderAsync(CreateRazorShape);
        await RenderAsync(CreateLiquidShape);
    }

    [GlobalCleanup]
    public void Cleanup() => _context?.Dispose();

    private Task RenderAsync(Func<IShapeFactory, ValueTask<IShape>> createShape)
    {
        return _context.UsingTenantScopeAsync(async scope =>
        {
            var shapeFactory = scope.ServiceProvider.GetRequiredService<IShapeFactory>();
            var renderer = scope.ServiceProvider.GetRequiredService<IShapeHtmlRenderer>();

            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                var shape = await createShape(shapeFactory);
                var html = await renderer.RenderAsync(shape);

                if (html.Length == 0)
                {
                    throw new InvalidOperationException("The shape rendered nothing.");
                }
            }
        });
    }

    // Pre-rendered content is returned by the display helper before the pipeline runs: this measures the harness alone,
    // i.e. what IShapeHtmlRenderer and the tenant scope cost around each shape.
    private static ValueTask<IShape> CreatePreRenderedShape(IShapeFactory factory)
        => ValueTask.FromResult((IShape)PositionWrapper.TryWrap(new HtmlString("<p>Hello</p>"), string.Empty));

    // 'DateTime' is a [Shape] method of OrchardCore.DisplayManagement: the pipeline without any template engine.
    private static ValueTask<IShape> CreateCodeShape(IShapeFactory factory)
        => factory.CreateAsync("DateTime", Arguments.From(new { Utc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }));

    // A call to 'CreateAsync<TitlePartViewModel>()' would be intercepted by the shape source generator, which would emit
    // in this assembly the same generated shape type as the one of OrchardCore.Title (CS0436). Through a delegate, the
    // runtime path creates a proxy of the model instead, which costs about 70 ns more, negligible for this measure.
    private static readonly Func<IShapeFactory, string, Action<TitlePartViewModel>, ValueTask<IShape>> s_createTitlePartShape = ShapeFactoryRuntimePath.GetCreateAsync<TitlePartViewModel>();

    // 'TitlePart.cshtml' of OrchardCore.Title: '<h1>@Model.Title</h1>', the smallest Razor template.
    private static ValueTask<IShape> CreateRazorShape(IShapeFactory factory)
        => s_createTitlePartShape(factory, "TitlePart", static m => m.Title = "Hello");

    // 'Search-Form.liquid' of TheBlogTheme: a form reading three properties of the shape.
    private static ValueTask<IShape> CreateLiquidShape(IShapeFactory factory)
        => factory.CreateAsync("Search__Form", Arguments.From(new { Index = "Search", Terms = "hello" }));

    [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke, Description = "Harness only (pre-rendered content)")]
    public Task PreRendered() => RenderAsync(CreatePreRenderedShape);

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke, Description = "Code binding ([Shape] DateTime)")]
    public Task Code() => RenderAsync(CreateCodeShape);

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke, Description = "Razor template (TitlePart.cshtml, typed)")]
    public Task Razor() => RenderAsync(CreateRazorShape);

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke, Description = "Liquid template (Search-Form.liquid)")]
    public Task Liquid() => RenderAsync(CreateLiquidShape);
}
