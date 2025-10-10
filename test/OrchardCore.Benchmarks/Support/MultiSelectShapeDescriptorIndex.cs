using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using Xunit.Internal;

namespace OrchardCore.Benchmarks.Support;

public class MultiSelectShapeDescriptorIndex : ShapeDescriptor
{
    private readonly ConcurrentDictionary<string, FeatureShapeDescriptor> _descriptors;
    private readonly List<FeatureShapeDescriptor> _alternationDescriptors;
    private readonly List<string> _wrappers;
    private readonly List<string> _bindingSources;
    private readonly List<Func<ShapeCreatingContext, Task>> _creatingAsync;
    private readonly List<Func<ShapeCreatedContext, Task>> _createdAsync;
    private readonly List<Func<ShapeDisplayContext, Task>> _displayingAsync;
    private readonly List<Func<ShapeDisplayContext, Task>> _processingAsync;
    private readonly List<Func<ShapeDisplayContext, Task>> _displayedAsync;

    public MultiSelectShapeDescriptorIndex(
        string shapeType,
        IEnumerable<string> alterationKeys,
        ConcurrentDictionary<string, FeatureShapeDescriptor> descriptors)
    {
        ArgumentException.ThrowIfNullOrEmpty(shapeType);

        ShapeType = shapeType;
        _descriptors = descriptors;

        // pre-calculate as much as we can
        _alternationDescriptors = alterationKeys
            .Select(key => _descriptors[key])
            .ToList();

        _wrappers = _alternationDescriptors
            .SelectMany(sd => sd.Wrappers)
            .ToList();

        _bindingSources = _alternationDescriptors
            .SelectMany(sd => sd.BindingSources)
            .ToList();

        foreach (var kv in _alternationDescriptors
            .SelectMany(sd => sd.Bindings)
            .GroupBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => kv.Last()))
        {
            Bindings[kv.Key] = kv.Value;
        }

        _creatingAsync = _alternationDescriptors
            .SelectMany(sd => sd.CreatingAsync)
            .ToList();

        _createdAsync = _alternationDescriptors
            .SelectMany(sd => sd.CreatedAsync)
            .ToList();

        _displayingAsync = _alternationDescriptors
            .SelectMany(sd => sd.DisplayingAsync)
            .ToList();

        _processingAsync = _alternationDescriptors
            .SelectMany(sd => sd.ProcessingAsync)
            .ToList();

        _displayedAsync = _alternationDescriptors
            .SelectMany(sd => sd.DisplayedAsync)
            .ToList();
    }

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

public class MultiSelectShapeDescriptorIndexArray : ShapeDescriptor
{
    private readonly ConcurrentDictionary<string, FeatureShapeDescriptor> _descriptors;
    private readonly FeatureShapeDescriptor[] _alternationDescriptors;
    private readonly string[] _wrappers;
    private readonly string[] _bindingSources;
    private readonly Func<ShapeCreatingContext, Task>[] _creatingAsync;
    private readonly Func<ShapeCreatedContext, Task>[] _createdAsync;
    private readonly Func<ShapeDisplayContext, Task>[] _displayingAsync;
    private readonly Func<ShapeDisplayContext, Task>[] _processingAsync;
    private readonly Func<ShapeDisplayContext, Task>[] _displayedAsync;

    public MultiSelectShapeDescriptorIndexArray(
        string shapeType,
        IEnumerable<string> alterationKeys,
        ConcurrentDictionary<string, FeatureShapeDescriptor> descriptors)
    {
        ArgumentException.ThrowIfNullOrEmpty(shapeType);

        ShapeType = shapeType;
        _descriptors = descriptors;

        // pre-calculate as much as we can
        _alternationDescriptors = alterationKeys
            .Select(key => _descriptors[key])
            .ToArray();

        _wrappers = _alternationDescriptors
            .SelectMany(sd => sd.Wrappers)
            .ToArray();

        _bindingSources = _alternationDescriptors
            .SelectMany(sd => sd.BindingSources)
            .ToArray();

        foreach (var kv in _alternationDescriptors
            .SelectMany(sd => sd.Bindings)
            .GroupBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => kv.Last()))
        {
            Bindings[kv.Key] = kv.Value;
        }

        _creatingAsync = _alternationDescriptors
            .SelectMany(sd => sd.CreatingAsync)
            .ToArray();

        _createdAsync = _alternationDescriptors
            .SelectMany(sd => sd.CreatedAsync)
            .ToArray();

        _displayingAsync = _alternationDescriptors
            .SelectMany(sd => sd.DisplayingAsync)
            .ToArray();

        _processingAsync = _alternationDescriptors
            .SelectMany(sd => sd.ProcessingAsync)
            .ToArray();

        _displayedAsync = _alternationDescriptors
            .SelectMany(sd => sd.DisplayedAsync)
            .ToArray();
    }

    public override IReadOnlyList<Func<ShapeCreatingContext, Task>> CreatingAsync => _creatingAsync;

    public override IReadOnlyList<Func<ShapeCreatedContext, Task>> CreatedAsync => _createdAsync;

    public override IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayingAsync => _displayingAsync;

    public override IReadOnlyList<Func<ShapeDisplayContext, Task>> ProcessingAsync => _processingAsync;

    public override IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayedAsync => _displayedAsync;

    public override Func<ShapePlacementContext, PlacementInfo> Placement => CalculatePlacement;

    private PlacementInfo CalculatePlacement(ShapePlacementContext ctx)
    {
        PlacementInfo info = null;
        for (var i = _alternationDescriptors.Length - 1; i >= 0; i--)
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
