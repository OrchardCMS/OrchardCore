using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Descriptors;

public sealed class FeatureShapeDescriptor : ShapeDescriptor
{
    public FeatureShapeDescriptor(IFeatureInfo feature, string shapeType)
    {
        Feature = feature;
        ShapeType = shapeType;
    }

    public IFeatureInfo Feature { get; }
}

public sealed class ShapeDescriptorIndex : ShapeDescriptor
{
    private readonly List<FeatureShapeDescriptor> _alterationDescriptors;
    private readonly IReadOnlyList<string> _wrappers;
    private readonly IReadOnlyList<Func<ShapeCreatingContext, Task>> _creatingAsync;
    private readonly IReadOnlyList<Func<ShapeCreatedContext, Task>> _createdAsync;
    private readonly IReadOnlyList<Func<ShapeDisplayContext, Task>> _displayingAsync;
    private readonly IReadOnlyList<Func<ShapeDisplayContext, Task>> _processingAsync;
    private readonly IReadOnlyList<Func<ShapeDisplayContext, Task>> _displayedAsync;

    public ShapeDescriptorIndex(
        string shapeType,
        IEnumerable<FeatureShapeDescriptor> alterations)
    {
        ArgumentException.ThrowIfNullOrEmpty(shapeType);

        ShapeType = shapeType;

        List<string> wrappers = null;
        List<Func<ShapeCreatingContext, Task>> creatingAsync = null;
        List<Func<ShapeCreatedContext, Task>> createdAsync = null;
        List<Func<ShapeDisplayContext, Task>> displayingAsync = null;
        List<Func<ShapeDisplayContext, Task>> processingAsync = null;
        List<Func<ShapeDisplayContext, Task>> displayedAsync = null;
        var bindingSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Pre-calculate as much as we can for performance reasons.
        foreach (var alterationDescriptor in alterations)
        {
            _alterationDescriptors ??= [];
            _alterationDescriptors.Add(alterationDescriptor);

            if (alterationDescriptor.Wrappers.Count > 0)
            {
                wrappers ??= [];
                wrappers.AddRange(alterationDescriptor.Wrappers);
            }

            if (alterationDescriptor.CreatingAsync.Count > 0)
            {
                creatingAsync ??= [];
                creatingAsync.AddRange(alterationDescriptor.CreatingAsync);
            }

            if (alterationDescriptor.CreatedAsync.Count > 0)
            {
                createdAsync ??= [];
                createdAsync.AddRange(alterationDescriptor.CreatedAsync);
            }

            if (alterationDescriptor.DisplayingAsync.Count > 0)
            {
                displayingAsync ??= [];
                displayingAsync.AddRange(alterationDescriptor.DisplayingAsync);
            }

            if (alterationDescriptor.ProcessingAsync.Count > 0)
            {
                processingAsync ??= [];
                processingAsync.AddRange(alterationDescriptor.ProcessingAsync);
            }

            if (alterationDescriptor.DisplayedAsync.Count > 0)
            {
                displayedAsync ??= [];
                displayedAsync.AddRange(alterationDescriptor.DisplayedAsync);
            }

            foreach (var binding in alterationDescriptor.Bindings)
            {
                // Only add the first binding for each binding source. This ensures that only the
                // first binding of a extension is used, and that overrides are not ignored.
                if (bindingSources.Add(binding.Value.BindingSource))
                {
                    Bindings[binding.Key] = binding.Value;
                }
            }
        }

        _wrappers = wrappers;
        _creatingAsync = creatingAsync;
        _createdAsync = createdAsync;
        _displayingAsync = displayingAsync;
        _processingAsync = processingAsync;
        _displayedAsync = displayedAsync;

        // Ensure none of these are null. This is done separately to make sure the []
        // operator is converted to Array.Empty<T>() at compile time.
        _wrappers ??= [];
        _creatingAsync ??= [];
        _createdAsync ??= [];
        _displayingAsync ??= [];
        _processingAsync ??= [];
        _displayedAsync ??= [];
    }

    public override IReadOnlyList<Func<ShapeCreatingContext, Task>> CreatingAsync => _creatingAsync;

    public override IReadOnlyList<Func<ShapeCreatedContext, Task>> CreatedAsync => _createdAsync;

    public override IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayingAsync => _displayingAsync;

    public override IReadOnlyList<Func<ShapeDisplayContext, Task>> ProcessingAsync => _processingAsync;

    public override IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayedAsync => _displayedAsync;

    public override Func<ShapePlacementContext, PlacementInfo> Placement => CalculatePlacement;

    public override IReadOnlyList<string> Wrappers => _wrappers;

    private PlacementInfo CalculatePlacement(ShapePlacementContext ctx)
    {
        if (_alterationDescriptors == null)
        {
            return DefaultPlacementAction(ctx);
        }

        PlacementInfo info = null;
        for (var i = _alterationDescriptors.Count - 1; i >= 0; i--)
        {
            var descriptor = _alterationDescriptors[i];
            info = descriptor.Placement(ctx);
            if (info != null)
            {
                break;
            }
        }

        return info ?? DefaultPlacementAction(ctx);
    }
}

public class ShapeDescriptor
{
    private Func<ShapePlacementContext, PlacementInfo> _placement;

    public string ShapeType { get; set; }

    /// <summary>
    /// The BindingSource is informational text about the source of the Binding delegate. Not used except for
    /// troubleshooting.
    /// </summary>
    public string BindingSource
        => Bindings.TryGetValue(ShapeType, out var binding) ? binding.BindingSource : null;

    public Func<DisplayContext, Task<IHtmlContent>> Binding
        => Bindings.TryGetValue(ShapeType, out var binding) ? binding.BindingAsync : null;

    public IDictionary<string, ShapeBinding> Bindings { get; } = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase);

    public virtual IReadOnlyList<Func<ShapeCreatingContext, Task>> CreatingAsync { get; set; } = [];

    public virtual IReadOnlyList<Func<ShapeCreatedContext, Task>> CreatedAsync { get; set; } = [];

    public virtual IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayingAsync { get; set; } = [];

    public virtual IReadOnlyList<Func<ShapeDisplayContext, Task>> ProcessingAsync { get; set; } = [];

    public virtual IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayedAsync { get; set; } = [];

    public virtual Func<ShapePlacementContext, PlacementInfo> Placement { get => _placement ??= DefaultPlacementAction; set => _placement = value; }

    public string DefaultPlacement { get; set; }

    public virtual IReadOnlyList<string> Wrappers { get; set; } = [];

    protected PlacementInfo DefaultPlacementAction(ShapePlacementContext context)
    {
        // A null default placement means no default placement is specified
        if (DefaultPlacement == null)
        {
            return null;
        }

        return new PlacementInfo
        {
            Location = DefaultPlacement,
        };
    }
}

public sealed class ShapeBinding
{
    public string BindingName { get; set; }
    public string BindingSource { get; set; }
    public Func<DisplayContext, Task<IHtmlContent>> BindingAsync { get; set; }
}
