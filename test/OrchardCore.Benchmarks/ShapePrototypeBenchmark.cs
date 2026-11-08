using System.IO;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using OrchardCore.Benchmarks.ShapePrototype;

namespace OrchardCore.Benchmarks;

/// <summary>
/// The same scenarios as <see cref="ShapePipelineBenchmark"/>, on the prototype of the proposed model:
/// plain models, nodes without dynamic behavior, binding resolution cached by shape key, compiled placement
/// rules and streaming rendering. The templates are code delegates writing the same markup.
/// </summary>
[MemoryDiagnoser]
public class ShapePrototypeBenchmark
{
    // The same alternates as in ShapePipelineBenchmark, computed once and shared, as a factory cache would.
    private static readonly AlternateSet s_partAlternates = new(
    [
        "TitlePart_Summary",
        "BlogPost__TitlePart",
        "BlogPost_Summary__TitlePart",
        "Content__TitlePart",
        "Content_Summary__TitlePart",
    ]);

    private TemplateTable _templates;
    private PlacementTable _placement;
    private ShapeRenderer _renderer;
    private readonly StringBuilder _buffer = new(4096);
    private StringWriter _output;

    public class TitlePartViewModel
    {
        public string Title { get; set; }
    }

    [GlobalSetup]
    public void Setup()
    {
        _output = new StringWriter(_buffer);

        _templates = new TemplateTable();
        _templates.Add("TitlePart", new DelegateTemplate(static ctx =>
        {
            ctx.Output.Write("<p>Hello</p>");
            return ValueTask.CompletedTask;
        }));
        _templates.Add("TextField", new DelegateTemplate(static ctx =>
        {
            ctx.Output.Write("<p>Hello</p>");
            return ValueTask.CompletedTask;
        }));
        _templates.Add("Wrapper", new DelegateTemplate(static async ctx =>
        {
            ctx.Output.Write("<div>");
            await ctx.RenderChildContentAsync();
            ctx.Output.Write("</div>");
        }));
        _templates.Add("Zone", new DelegateTemplate(static async ctx =>
        {
            foreach (var child in ctx.Shape.Children)
            {
                await ctx.RenderAsync(child);
            }
        }));
        _templates.Add("Content", new DelegateTemplate(static async ctx =>
        {
            ctx.Output.Write("<article>");
            await ctx.RenderZoneAsync("Header");
            await ctx.RenderZoneAsync("Content");
            ctx.Output.Write("</article>");
        }));

        _placement = new PlacementTable();
        _placement.Add("TitlePart", new PlacementRule { DisplayType = "Detail", Location = "Header:5" });
        _placement.Add("TitlePart", new PlacementRule { Differentiator = "Other", Location = "-" });

        _renderer = new ShapeRenderer(_templates);
    }

    private static ShapeNode NewPartShape(string type, bool withAlternates, bool withWrapper = false)
        => new(type, new TitlePartViewModel { Title = "Hello" })
        {
            DisplayType = "Summary",
            Name = type,
            Differentiator = type,
            Alternates = withAlternates ? s_partAlternates : null,
            Wrappers = withWrapper ? ["Wrapper"] : null,
        };

    [Benchmark(Description = "Create: node + model")]
    public ShapeNode CreateShape() => NewPartShape("TitlePart", withAlternates: true);

    [Benchmark(Description = "Render leaf: no alternates")]
    public ValueTask RenderLeaf()
    {
        _buffer.Clear();
        return _renderer.RenderAsync(NewPartShape("TitlePart", withAlternates: false), _output);
    }

    [Benchmark(Description = "Render leaf: 5 alternates, none bound")]
    public ValueTask RenderLeafWithAlternates()
    {
        _buffer.Clear();
        return _renderer.RenderAsync(NewPartShape("TitlePart", withAlternates: true), _output);
    }

    [Benchmark(Description = "Render leaf: 5 alternates + 1 wrapper")]
    public ValueTask RenderLeafWithWrapper()
    {
        _buffer.Clear();
        return _renderer.RenderAsync(NewPartShape("TitlePart", withAlternates: true, withWrapper: true), _output);
    }

    [Benchmark(Description = "Place: one driver result (model + placement rules)")]
    public ShapeNode PlaceDriverResult()
    {
        var content = new ShapeNode("Content");
        return ShapePlacer.Place(content, _placement, "TitlePart", "Summary", "TitlePart", "Content:5", s_partAlternates, new TitlePartViewModel { Title = "Hello" });
    }

    [Benchmark(Description = "Content tree: build (6 results) and render 9 shapes")]
    public async Task BuildAndRenderContentTree()
    {
        _buffer.Clear();

        var content = new ShapeNode("Content") { DisplayType = "Summary" };

        ShapePlacer.Place(content, _placement, "TitlePart", "Summary", "TitlePart", "Header:5", s_partAlternates, new TitlePartViewModel { Title = "Hello" });
        ShapePlacer.Place(content, _placement, "TextField", "Summary", "TextField", "Content:1", s_partAlternates, new TitlePartViewModel { Title = "Hello" });
        ShapePlacer.Place(content, _placement, "TextField", "Summary", "TextField", "Content:2", s_partAlternates, new TitlePartViewModel { Title = "Hello" });
        ShapePlacer.Place(content, _placement, "TextField", "Summary", "TextField", "Content:3", s_partAlternates, new TitlePartViewModel { Title = "Hello" });
        ShapePlacer.Place(content, _placement, "TextField", "Summary", "TextField", "Content:4", s_partAlternates, new TitlePartViewModel { Title = "Hello" });
        ShapePlacer.Place(content, _placement, "TextField", "Summary", "TextField", "Content:5", s_partAlternates, new TitlePartViewModel { Title = "Hello" });

        await _renderer.RenderAsync(content, _output);
    }
}
