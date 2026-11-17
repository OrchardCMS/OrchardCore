using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Benchmarks;

/// <summary>
/// Measures how templates read shape data: through the DLR on a dynamic <see cref="Shape"/>, through the
/// <see cref="IShape.Properties"/> dictionary, or through a typed view model property.
/// </summary>
[MemoryDiagnoser]
public class DynamicShapeAccessBenchmark
{
    private readonly Shape _shape = new();
    private readonly dynamic _dynamicShape;
    private readonly TitleModel _typed = new() { Title = "Hello" };
    private readonly ZoneHolding _layout = new(() => ValueTask.FromResult<IShape>(new Shape()));
    private readonly dynamic _dynamicLayout;

    public DynamicShapeAccessBenchmark()
    {
        _shape.Properties["Title"] = "Hello";
        _shape.Properties["ContentItem"] = new object();
        _dynamicShape = _shape;
        _dynamicLayout = _layout;
    }

    public class TitleModel
    {
        public string Title { get; set; }
    }

    [Benchmark(Baseline = true, Description = "Typed property")]
    public string TypedProperty() => _typed.Title;

    [Benchmark(Description = "IShape.Properties[\"Title\"]")]
    public object PropertiesDictionary() => _shape.Properties["Title"];

    [Benchmark(Description = "dynamic Model.Title (DLR)")]
    public object DynamicMember() => _dynamicShape.Title;

    [Benchmark(Description = "dynamic Model.Missing (DLR, falls back to Named())")]
    public object DynamicMissingMember() => _dynamicShape.Missing;

    [Benchmark(Description = "dynamic Layout.Header (ZoneHolding, on demand zone)")]
    public object DynamicZone() => _dynamicLayout.Header;

    [Benchmark(Description = "dynamic Model.Title = value (DLR set)")]
    public void DynamicSet() => _dynamicShape.Title = "Hello";
}
