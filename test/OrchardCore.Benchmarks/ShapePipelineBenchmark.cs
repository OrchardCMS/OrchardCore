using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Benchmarks.Support;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Modules.Manifest;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Benchmarks;

/// <summary>
/// Measures the cost of the shape pipeline itself (creation, placement, binding resolution and dispatch),
/// independently of any template engine: every binding is a code delegate that returns a static string.
/// The <c>DirectBinding</c> benchmark is the floor, it is what a page would pay if templates were called directly.
/// </summary>
/// <remarks>
/// The <c>CreateAsync&lt;TModel&gt;()</c> and <c>Initialize&lt;TModel&gt;()</c> calls of this project are intercepted
/// by the shape source generator, as they are in the modules: the typed shapes are generated classes, not Castle
/// DynamicProxy proxies. The proxy path, taken by the call sites the generator doesn't intercept, is measured by
/// calling the factory extension through a delegate, which the generator doesn't intercept, and in
/// <see cref="ShapeProxyBenchmark"/>.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage(
    "Performance",
    "CA1822:Mark members as static",
    Justification = "BenchmarkDotNet needs all benchmark methods to be instance-level.")]
public class ShapePipelineBenchmark
{
    private static readonly HtmlString s_html = new("<p>Hello</p>");

    // A realistic set of alternates for a content part shape rendered in the Summary display type,
    // as produced by ContentPartAlternatesFactory. None of them has a template, the base type is used.
    private static readonly string[] s_partAlternates =
    [
        "TitlePart_Summary",
        "BlogPost__TitlePart",
        "BlogPost_Summary__TitlePart",
        "Content__TitlePart",
        "Content_Summary__TitlePart",
    ];

    // A delegate invocation is not intercepted by the source generator, so this calls the runtime path of the
    // factory extension, which creates a Castle DynamicProxy proxy of the model.
    private static readonly Func<IShapeFactory, string, Action<TitlePartViewModel>, ValueTask<IShape>> s_createProxyShape = ShapeFactoryRuntimePath.GetCreateAsync<TitlePartViewModel>();

    private IServiceProvider _services;
    private IServiceProvider _servicesWithResolver;
    private IShapeFactory _factory;
    private IDisplayHelper _display;
    private IDisplayHelper _displayWithResolver;
    private ShapeBinding _titleBinding;
    private DisplayContext _directContext;
    private BuildDisplayContext _buildContext;
    private IShape _layout;
    private readonly StringBuilder _buffer = new(4096);
    private StringWriter _output;

    [GlobalSetup]
    public void Setup()
    {
        _output = new StringWriter(_buffer);
        var shapeTable = BuildShapeTable();

        _services = BuildServices(shapeTable, withResolver: false);
        _servicesWithResolver = BuildServices(shapeTable, withResolver: true);

        _factory = _services.GetRequiredService<IShapeFactory>();
        _display = _services.GetRequiredService<IDisplayHelper>();
        _displayWithResolver = _servicesWithResolver.GetRequiredService<IDisplayHelper>();

        _titleBinding = shapeTable.Bindings["TitlePart"];
        _directContext = new DisplayContext
        {
            DisplayHelper = _display,
            ServiceProvider = _services,
            Value = new Shape(),
        };

        _layout = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));

        _buildContext = CreateBuildContext(shapeTable);
    }

    private static ShapeTable BuildShapeTable()
    {
        var descriptors = new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase);
        var bindings = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase);

        void Add(ShapeDescriptor descriptor)
        {
            descriptors[descriptor.ShapeType] = descriptor;
            foreach (var binding in descriptor.Bindings)
            {
                bindings[binding.Key] = binding.Value;
            }
        }

        // Leaf shape: a code binding returning a constant, equivalent to the cheapest possible template.
        Add(Describe("TitlePart", _ => Task.FromResult<IHtmlContent>(s_html), descriptor =>
        {
            // A placement.json-like rule chain: two predicates evaluated for each placed shape.
            var first = descriptor.Placement;
            descriptor.Placement = ctx => (ctx.DisplayType == "Detail" ? PlacementInfo.FromLocation("Header:5") : null) ?? first(ctx);
            var second = descriptor.Placement;
            descriptor.Placement = ctx => (ctx.Differentiator == "Other" ? PlacementInfo.FromLocation("-") : null) ?? second(ctx);
        }));

        Add(Describe("TextField", _ => Task.FromResult<IHtmlContent>(s_html)));

        // A wrapper reads the child content it wraps.
        Add(Describe("Wrapper", ctx => Task.FromResult<IHtmlContent>(new HtmlContentBuilder().AppendHtml("<div>").AppendHtml(ctx.Value.Metadata.ChildContent).AppendHtml("</div>"))));

        // Containers render their children, as the Zone and ContentZone shapes do.
        Add(Describe("Zone", RenderItemsAsync));
        Add(Describe("ContentZone", RenderItemsAsync));

        // A content shape renders its zones, as Content.Summary.cshtml does.
        Add(Describe("Content", async ctx =>
        {
            var builder = new HtmlContentBuilder();
            var shape = ctx.Value;
            builder.AppendHtml("<article>");
            builder.AppendHtml(await ctx.DisplayHelper.ShapeExecuteAsync(shape.GetProperty<IShape>("Header")));
            builder.AppendHtml(await ctx.DisplayHelper.ShapeExecuteAsync(shape.GetProperty<IShape>("Content")));
            builder.AppendHtml("</article>");
            return builder;
        }));

        return new ShapeTable(descriptors, bindings);
    }

    private static async Task<IHtmlContent> RenderItemsAsync(DisplayContext ctx)
    {
        var builder = new HtmlContentBuilder();
        foreach (var item in ctx.Value.Items)
        {
            builder.AppendHtml(await ctx.DisplayHelper.ShapeExecuteAsync((IShape)item));
        }

        return builder;
    }

    private static ShapeDescriptor Describe(string shapeType, Func<DisplayContext, Task<IHtmlContent>> binding, Action<ShapeDescriptor> configure = null)
    {
        var descriptor = new ShapeDescriptor { ShapeType = shapeType };
        descriptor.Bindings[shapeType] = new ShapeBinding
        {
            BindingName = shapeType,
            BindingSource = shapeType + ".cshtml",
            BindingAsync = binding,
        };

        configure?.Invoke(descriptor);

        return descriptor;
    }

    private static ServiceProvider BuildServices(ShapeTable shapeTable, bool withResolver)
    {
        var services = new ServiceCollection();

        services.AddScoped<IHtmlDisplay, DefaultHtmlDisplay>();
        services.AddScoped<IDisplayHelper, DisplayHelper>();
        services.AddScoped<IShapeFactory, DefaultShapeFactory>();
        services.AddSingleton<IShapeTableManager>(new TestShapeTableManager(shapeTable));
        services.AddSingleton<IThemeManager>(new MockThemeManager(new ExtensionInfo("path", new ManifestInfo(new ModuleAttribute()), (x, y) => [])));
        services.AddSingleton(Mock.Of<IOptionsMonitor<ShapeRenderingOptions>>(x => x.CurrentValue == new ShapeRenderingOptions()));
        services.AddLogging();
        services.AddOptions();

        if (withResolver)
        {
            // Simulates the Templates feature: a resolver that is asked for every alternate of every shape.
            services.AddSingleton<IShapeBindingResolver, NullShapeBindingResolver>();
        }

        return services.BuildServiceProvider();
    }

    private BuildDisplayContext CreateBuildContext(ShapeTable shapeTable)
    {
        var content = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        var context = new BuildDisplayContext(content, "Summary", "", _factory, (IZoneHolding)_layout, null);

        // What BaseDisplayManager.BindPlacementAsync() wires up: a resolver over the shape table.
        context.FindPlacement = (shapeType, differentiator, displayType, ctx) =>
        {
            var placementContext = new ShapePlacementContext(shapeType, displayType, differentiator, ctx.Shape);
            return shapeTable.Descriptors.TryGetValue(shapeType, out var descriptor)
                ? descriptor.Placement(placementContext)
                : null;
        };

        return context;
    }

    private sealed class NullShapeBindingResolver : IShapeBindingResolver
    {
        public Task<ShapeBinding> GetShapeBindingAsync(string shapeType) => Task.FromResult<ShapeBinding>(null);
    }

    public class TitlePartViewModel
    {
        public string Title { get; set; }
    }

    private static ShapeViewModel NewPartShape(string type, bool withAlternates, bool withWrapper = false)
    {
        var shape = new ShapeViewModel(type);
        shape.Metadata.DisplayType = "Summary";
        shape.Metadata.Name = type;
        shape.Metadata.Differentiator = type;

        if (withAlternates)
        {
            shape.Metadata.Alternates.AddRange(s_partAlternates);
        }

        if (withWrapper)
        {
            shape.Metadata.Wrappers.Add("Wrapper");
        }

        return shape;
    }

    /// <summary>
    /// Writes the rendered content to the output, as a page does, so that the measures include the final copy.
    /// </summary>
    private async Task WriteAsync(Task<IHtmlContent> content)
    {
        _buffer.Clear();
        (await content).WriteTo(_output, HtmlEncoder.Default);
    }

    // --- Floor -------------------------------------------------------------------------------------------------

    [Benchmark(Baseline = true, Description = "Floor: call the binding delegate directly")]
    public Task DirectBinding() => WriteAsync(_titleBinding.BindingAsync(_directContext));

    // --- Creation ----------------------------------------------------------------------------------------------

    [Benchmark(Description = "Create: dynamic Shape through the factory")]
    public ValueTask<IShape> CreateDynamicShape() => _factory.CreateAsync("TitlePart");

    [Benchmark(Description = "Create: generated typed shape (CreateAsync<TModel>, source generator) through the factory")]
    public ValueTask<IShape> CreateGeneratedShape() => _factory.CreateAsync<TitlePartViewModel>("TitlePart", static m => m.Title = "Hello");

    [Benchmark(Description = "Create: Castle proxy shape (CreateAsync<TModel> without the source generator) through the factory")]
    public ValueTask<IShape> CreateProxyShape() => s_createProxyShape(_factory, "TitlePart", static m => m.Title = "Hello");

    [Benchmark(Description = "Create: new ShapeViewModel (no factory)")]
    public IShape CreateShapeViewModel() => new ShapeViewModel("TitlePart");

    // --- Rendering one leaf shape ------------------------------------------------------------------------------

    [Benchmark(Description = "Render leaf: no alternates")]
    public Task RenderLeaf() => WriteAsync(_display.ShapeExecuteAsync(NewPartShape("TitlePart", withAlternates: false)));

    [Benchmark(Description = "Render leaf: 5 alternates, none bound")]
    public Task RenderLeafWithAlternates() => WriteAsync(_display.ShapeExecuteAsync(NewPartShape("TitlePart", withAlternates: true)));

    [Benchmark(Description = "Render leaf: 5 alternates + Templates resolver")]
    public Task RenderLeafWithAlternatesAndResolver() => WriteAsync(_displayWithResolver.ShapeExecuteAsync(NewPartShape("TitlePart", withAlternates: true)));

    [Benchmark(Description = "Render leaf: 5 alternates + 1 wrapper")]
    public Task RenderLeafWithWrapper() => WriteAsync(_display.ShapeExecuteAsync(NewPartShape("TitlePart", withAlternates: true, withWrapper: true)));

    // --- Placing driver results --------------------------------------------------------------------------------

    [Benchmark(Description = "Place: ShapeResult.ApplyAsync (generated Initialize<TModel> + placement rules)")]
    public async Task PlaceDriverResult()
    {
        var result = new DisplayDriverStub().Build();
        await result.ApplyAsync(_buildContext);
    }

    // --- A content item -----------------------------------------------------------------------------------------

    [Benchmark(Description = "Content tree: build (6 results) and render 9 shapes")]
    public async Task BuildAndRenderContentTree()
    {
        var content = new ZoneHolding(() => ValueTask.FromResult<IShape>(new ShapeViewModel("ContentZone")));
        content.Metadata.Type = "Content";
        content.Metadata.DisplayType = "Summary";

        var context = new BuildDisplayContext(content, "Summary", "", _factory, (IZoneHolding)_layout, null)
        {
            FindPlacement = _buildContext.FindPlacement,
        };

        var driver = new DisplayDriverStub();

        await driver.Build("TitlePart", "Header:5").ApplyAsync(context);
        await driver.Build("TextField", "Content:1").ApplyAsync(context);
        await driver.Build("TextField", "Content:2").ApplyAsync(context);
        await driver.Build("TextField", "Content:3").ApplyAsync(context);
        await driver.Build("TextField", "Content:4").ApplyAsync(context);
        await driver.Build("TextField", "Content:5").ApplyAsync(context);

        await WriteAsync(_display.ShapeExecuteAsync(content));
    }

    private sealed class DisplayDriverStub : DisplayDriverBase
    {
        public ShapeResult Build() => Build("TitlePart", "Content:5");

        public ShapeResult Build(string shapeType, string location)
        {
            return Initialize<TitlePartViewModel>(shapeType, static m => m.Title = "Hello")
#pragma warning disable OCSG003 // The callers pass constant locations, none with a tab or a card.
                .Location(location)
#pragma warning restore OCSG003
                .Differentiator(shapeType)
                .Displaying(static ctx => ctx.Shape.Metadata.Alternates.AddRange(s_partAlternates));
        }
    }
}
