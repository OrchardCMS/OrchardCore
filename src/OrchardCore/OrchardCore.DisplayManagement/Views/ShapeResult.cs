using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DisplayManagement.Views;

public class ShapeResult : IDisplayResult
{
    private readonly string _shapeType;
    private readonly Func<IBuildShapeContext, ValueTask<IShape>> _shapeBuilder;
    private readonly Func<IShape, Task> _initializingAsync;

    private string _defaultLocation;
    private string _name;
    private string _differentiator;
    private string _prefix;
    private string _cacheId;
    private Dictionary<string, string> _otherLocations;
    private StringValues _groupIds;

    private Action<CacheContext> _cache;
    private Action<ShapeDisplayContext> _displaying;
    private Func<IShape, Task> _processingAsync;
    private Func<Task<bool>> _renderPredicateAsync;

    /// <summary>
    /// Creates a new instance of <see cref="ShapeResult"/>.
    /// </summary>
    /// <param name="shapeType">The Shape type used for the created Shape.</param>
    /// <param name="shapeBuilder">A delegate that creates the shape instance.</param>
    public ShapeResult(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder)
        : this(shapeType, shapeBuilder, null)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="ShapeResult"/>.
    /// </summary>
    /// <param name="shapeType">The Shape type used for the created Shape.</param>
    /// <param name="shapeBuilder">A delegate that creates the shape instance.</param>
    /// <param name="initializing">A delegate that is executed after the shape is created.</param>
    public ShapeResult(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder, Func<IShape, Task> initializing)
    {
        // The shape type is necessary before the shape is created as it will drive the placement
        // resolution which itself can prevent the shape from being created.

        _shapeType = shapeType;
        _shapeBuilder = shapeBuilder;
        _initializingAsync = initializing;
    }

    public Task ApplyAsync(BuildDisplayContext context)
    {
        return ApplyImplementationAsync(context, context.DisplayType);
    }

    public Task ApplyAsync(BuildEditorContext context)
    {
        return ApplyImplementationAsync(context, "Edit");
    }

    private async Task ApplyImplementationAsync(BuildShapeContext context, string displayType)
    {
        // If no location is set from the driver, use the one from the context.
        if (string.IsNullOrEmpty(_defaultLocation))
        {
            _defaultLocation = context.DefaultZone;
        }

        // Look into specific implementations of placements (like placement.json files and IShapePlacementProviders).
        var placement = context.FindPlacement(_shapeType, _differentiator, displayType, context);

        // Look for mapped display type locations.
        if (_otherLocations != null)
        {
            string displayTypePlacement;
            if (_otherLocations.TryGetValue(displayType, out displayTypePlacement))
            {
                _defaultLocation = displayTypePlacement;
            }
        }

        // If no placement is found, use the default location.
        placement ??= new PlacementInfo
        {
            Location = _defaultLocation,
        };

        // If a placement was found without actual location, use the default.
        // It can happen when just setting alternates or wrappers for instance.
        placement.Location ??= _defaultLocation;

        placement.DefaultPosition ??= context.DefaultPosition;

        // If the placement should be hidden, then stop rendering execution.
        if (placement.IsHidden())
        {
            return;
        }

        // Parse group placement.
        var groupId = placement.GetGroup();

        // Apply group constraints from placement
        if (!string.IsNullOrEmpty(groupId))
        {
            OnGroup(groupId);
        }

        bool hasGroupConstraints = !StringValues.IsNullOrEmpty(_groupIds);

        // If no specific group is requested, use "" as it represents "any group" when applied on a shape.
        // This allows to render shapes when no shape constraints are set and also on specific groups.
        var requestedGroup = context.GroupId ?? string.Empty;

        // If the shape's group doesn't match the currently rendered one, return.
        if (hasGroupConstraints && !_groupIds.Contains(requestedGroup, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        // If we try to render the shape without a group, but we require one, don't render it
        if (!hasGroupConstraints && !string.IsNullOrEmpty(context.GroupId))
        {
            return;
        }

        // If a condition has been applied to this result evaluate it only if the shape has been placed.
        if (_renderPredicateAsync != null && !await _renderPredicateAsync())
        {
            return;
        }

        var newShape = Shape = await _shapeBuilder(context);

        // Ignore it if the driver returned a null shape.
        if (newShape == null)
        {
            return;
        }

        var newShapeMetadata = newShape.Metadata;
        newShapeMetadata.Prefix = _prefix;
        newShapeMetadata.Name = _name ?? _differentiator ?? _shapeType;
        newShapeMetadata.Differentiator = _differentiator ?? _shapeType;
        newShapeMetadata.DisplayType = displayType;
        newShapeMetadata.PlacementSource = placement.Source;
        newShapeMetadata.Tab = placement.GetTab();
        newShapeMetadata.Card = placement.GetCard();
        newShapeMetadata.Column = placement.GetColumn();
        newShapeMetadata.Type = _shapeType;

        // Invoke the initialization code first when all Displaying events are invoked.
        // These Displaying methods are used to create alternates for instance, so the
        // Shape needs to have required properties available first.

        if (_initializingAsync != null)
        {
            await _initializingAsync.Invoke(Shape);
        }

        if (_displaying != null)
        {
            newShapeMetadata.OnDisplaying(_displaying);
        }

        if (_processingAsync != null)
        {
            newShapeMetadata.OnProcessing(_processingAsync);
        }

        // Apply cache settings
        if (!string.IsNullOrEmpty(_cacheId) && _cache != null)
        {
            _cache(newShapeMetadata.Cache(_cacheId));
        }

        // If a specific shape is provided, remove all previous alternates and wrappers.
        if (!string.IsNullOrEmpty(placement.ShapeType))
        {
            newShapeMetadata.Type = placement.ShapeType;
            newShapeMetadata.Alternates.Clear();
            newShapeMetadata.Wrappers.Clear();
        }

        if (placement != null)
        {
            if (placement.Alternates != null)
            {
                newShapeMetadata.Alternates.AddRange(placement.Alternates);
            }

            if (placement.Wrappers != null)
            {
                newShapeMetadata.Wrappers.AddRange(placement.Wrappers);
            }
        }

        var parentShape = context.Shape;

        if (placement.IsLayoutZone())
        {
            parentShape = context.Layout;
        }

        var position = placement.GetPosition();
        var zones = placement.GetZones();

        foreach (var zone in zones)
        {
            if (parentShape == null)
            {
                break;
            }

            if (parentShape is IZoneHolding layout)
            {
                // parentShape is a ZoneHolding.
                parentShape = layout.Zones[zone];
            }
            else
            {
                // try to access it as a member.
                parentShape = parentShape.GetProperty<IShape>(zone);
            }
        }

        position = !string.IsNullOrEmpty(position) ? position : null;

        if (parentShape is Shape shape)
        {
            await shape.AddAsync(newShape, position);
        }
    }

    /// <summary>
    /// Sets the prefix of the form elements rendered in the shape.
    /// </summary>
    /// <remarks>
    /// The goal is to isolate each shape when edited together.
    /// </remarks>
    public ShapeResult Prefix(string prefix)
    {
        _prefix = prefix;
        return this;
    }

    /// <summary>
    /// Sets the default location of the shape when no specific placement applies.
    /// </summary>
    public ShapeResult Location(string location)
    {
        _defaultLocation = location;
        return this;
    }

    /// <summary>
    /// Sets the location to use for a matching display type.
    /// </summary>
    public ShapeResult Location(string displayType, string location)
    {
        _otherLocations ??= new Dictionary<string, string>(2);
        _otherLocations[displayType] = location;
        return this;
    }

    /// <summary>
    /// Sets the delegate to be executed when the shape is being displayed.
    /// </summary>
    public ShapeResult Displaying(Action<ShapeDisplayContext> displaying)
    {
        _displaying = displaying;

        return this;
    }

    /// <summary>
    /// Sets the delegate to be executed when the shape is rendered (not cached).
    /// </summary>
    public ShapeResult Processing(Func<IShape, Task> processing)
    {
        _processingAsync = processing;

        return this;
    }

    /// <summary>
    /// Sets the delegate to be executed when the shape is rendered (not cached).
    /// </summary>
    public ShapeResult Processing<T>(Func<T, Task> processing)
    {
        _processingAsync = shape => processing?.Invoke((T)shape);

        return this;
    }

    /// <summary>
    /// Sets the shape name regardless its 'Differentiator'.
    /// </summary>
    public ShapeResult Name(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets a discriminator that is used to find the location of the shape when two shapes of the same type are displayed.
    /// </summary>
    public ShapeResult Differentiator(string differentiator)
    {
        _differentiator = differentiator;
        return this;
    }

    /// <summary>
    /// Sets the group identifiers the shape will be rendered in.
    /// </summary>
    /// <param name="groupIds"></param>
    /// <returns></returns>
    public ShapeResult OnGroup(params string[] groupIds)
    {
        ArgumentNullException.ThrowIfNull(groupIds);

        _groupIds = StringValues.Concat(_groupIds, groupIds);

        return this;
    }

    /// <summary>
    /// Sets the caching properties of the shape to render.
    /// </summary>
    public ShapeResult Cache(string cacheId, Action<CacheContext> cache = null)
    {
        _cacheId = cacheId;
        _cache = cache;
        return this;
    }

    /// <summary>
    /// Sets a condition that must return true for the shape to render.
    /// The condition is only evaluated if the shape has been placed.
    /// </summary>
    public ShapeResult RenderWhen(Func<Task<bool>> renderPredicateAsync)
    {
        _renderPredicateAsync = renderPredicateAsync;
        return this;
    }

    public IShape Shape { get; private set; }
}
