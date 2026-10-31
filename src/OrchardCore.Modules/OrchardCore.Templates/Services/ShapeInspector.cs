using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Records the shapes rendered while it is recording, with their alternates, to show which templates render a content item.
/// It does nothing otherwise.
/// </summary>
public sealed class ShapeInspector : IShapeDisplayEvents
{
    private List<InspectedShape> _shapes;
    private Stack<InspectedShape> _rendering;

    /// <summary>
    /// Starts recording the rendered shapes.
    /// </summary>
    public void Start()
    {
        _shapes = [];
        _rendering = new Stack<InspectedShape>();
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <returns>The rendered shapes, in the order they started to render, each one before the shapes it renders.</returns>
    public IReadOnlyList<InspectedShape> Stop()
    {
        var shapes = _shapes ?? [];

        _shapes = null;
        _rendering = null;

        return shapes;
    }

    public Task DisplayingAsync(ShapeDisplayContext context)
    {
        if (_shapes != null)
        {
            var shape = new InspectedShape
            {
                Type = context.Shape.Metadata.Type,
                Depth = _rendering.Count,
            };

            _shapes.Add(shape);
            _rendering.Push(shape);
        }

        return Task.CompletedTask;
    }

    public Task DisplayedAsync(ShapeDisplayContext context) => Task.CompletedTask;

    // The alternates are complete once the shape is rendered, its displaying events adding some of them.
    public Task DisplayingFinalizedAsync(ShapeDisplayContext context)
    {
        if (_shapes != null && _rendering.Count > 0)
        {
            var shape = _rendering.Pop();
            var metadata = context.Shape.Metadata;

            shape.DisplayType = metadata.DisplayType;
            shape.Differentiator = metadata.Differentiator;
            shape.Alternates = metadata.Alternates.Reverse().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        return Task.CompletedTask;
    }
}
