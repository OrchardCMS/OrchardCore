using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;

namespace OrchardCore.Benchmarks.Support;

public class OriginalShapeDescriptorIndex : OriginalShapeDescriptor
{
    private readonly List<FeatureShapeDescriptor> _alternationDescriptors = [];
    private readonly List<string> _wrappers = [];
    private readonly List<string> _bindingSources = [];
    private readonly Dictionary<string, ShapeBinding> _bindings = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Func<ShapeCreatingContext, Task>> _creatingAsync = [];
    private readonly List<Func<ShapeCreatedContext, Task>> _createdAsync = [];
    private readonly List<Func<ShapeDisplayContext, Task>> _displayingAsync = [];
    private readonly List<Func<ShapeDisplayContext, Task>> _processingAsync = [];
    private readonly List<Func<ShapeDisplayContext, Task>> _displayedAsync = [];

    public OriginalShapeDescriptorIndex(
        string shapeType,
        IEnumerable<string> alterationKeys,
        ConcurrentDictionary<string, FeatureShapeDescriptor> descriptors)
    {
        ArgumentException.ThrowIfNullOrEmpty(shapeType);

        ShapeType = shapeType;

        // Pre-calculate as much as we can for performance reasons.
        foreach (var alterationKey in alterationKeys)
        {
            if (!descriptors.TryGetValue(alterationKey, out var alternationDescriptor))
            {
                continue;
            }

            _alternationDescriptors.Add(alternationDescriptor);
            _wrappers.AddRange(alternationDescriptor.Wrappers);
            // BindingSources were removed from FeatureShapeDescriptor.
            // Keep an empty list for compatibility with original benchmark API surface.
            _creatingAsync.AddRange(alternationDescriptor.CreatingAsync);
            _createdAsync.AddRange(alternationDescriptor.CreatedAsync);
            _displayingAsync.AddRange(alternationDescriptor.DisplayingAsync);
            _displayedAsync.AddRange(alternationDescriptor.DisplayedAsync);
            _processingAsync.AddRange(alternationDescriptor.ProcessingAsync);

            foreach (var binding in alternationDescriptor.Bindings)
            {
                _bindings[binding.Key] = binding.Value;
            }
        }
    }

    /// <summary>
    /// The BindingSource is informational text about the source of the Binding delegate. Not used except for
    /// troubleshooting.
    /// </summary>
    public override string BindingSource =>
        Bindings.TryGetValue(ShapeType, out var binding) ? binding.BindingSource : null;

    public override Func<DisplayContext, Task<IHtmlContent>> Binding =>
        Bindings.TryGetValue(ShapeType, out var binding) ? binding.BindingAsync : null;

    public override IDictionary<string, ShapeBinding> Bindings => _bindings;

    public override IReadOnlyList<Func<ShapeCreatingContext, Task>> CreatingAsync => _creatingAsync;

    public override IReadOnlyList<Func<ShapeCreatedContext, Task>> CreatedAsync => _createdAsync;

    public override IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayingAsync => _displayingAsync;

    public override IReadOnlyList<Func<ShapeDisplayContext, Task>> ProcessingAsync => _processingAsync;

    public override IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayedAsync => _displayedAsync;

    public override Func<ShapePlacementContext, PlacementInfo> Placement => CalculatePlacement;

    private PlacementInfo CalculatePlacement(ShapePlacementContext ctx)
    {
        PlacementInfo info = null;
        for (var i = _alternationDescriptors.Count - 1; i >= 0; i--)
        {
            var descriptor = _alternationDescriptors[i];
            info = descriptor.Placement(ctx);
            if (info != null)
            {
                break;
            }
        }

        return info ?? DefaultPlacementAction(ctx);
    }

    public override IReadOnlyList<string> Wrappers => _wrappers;

    public override IReadOnlyList<string> BindingSources => _bindingSources;
}

