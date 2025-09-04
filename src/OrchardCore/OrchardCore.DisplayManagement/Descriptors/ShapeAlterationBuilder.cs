using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Descriptors;

public class ShapeAlterationBuilder
{
    private IFeatureInfo _feature;
    private readonly string _shapeType;
    private readonly string _bindingName;
    private readonly List<Action<ShapeDescriptor>> _configurations = [];

    public ShapeAlterationBuilder(IFeatureInfo feature, string shapeType)
    {
        _feature = feature;
        _bindingName = shapeType;
        var delimiterIndex = shapeType.IndexOf("__", StringComparison.Ordinal);

        if (delimiterIndex < 0)
        {
            _shapeType = shapeType;
        }
        else
        {
            _shapeType = shapeType[..delimiterIndex];
        }
    }

    public ShapeAlterationBuilder From(IFeatureInfo feature)
    {
        _feature = feature;
        return this;
    }

    public ShapeAlterationBuilder Configure(Action<ShapeDescriptor> action)
    {
        _configurations.Add(action);
        return this;
    }

    public ShapeAlterationBuilder BoundAs(string bindingSource, Func<DisplayContext, Task<IHtmlContent>> bindingDelegate)
    {
        ArgumentException.ThrowIfNullOrEmpty(bindingSource);

        // Schedule the configuration.
        return Configure(descriptor =>
        {
            var binding = new ShapeBinding
            {
                BindingName = _bindingName,
                BindingSource = bindingSource,
                BindingAsync = bindingDelegate,
            };

            // ShapeDescriptor.Bindings is a case insensitive dictionary.
            descriptor.Bindings[_bindingName] = binding;
            descriptor.BindingSources = [.. descriptor.BindingSources, bindingSource];
        });
    }

    /// <summary>
    /// Called when the shape is being created.
    /// </summary>
    public ShapeAlterationBuilder OnCreating(Action<ShapeCreatingContext> action)
    {
        return OnCreating(ctx =>
        {
            action(ctx);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Called when the shape is being created.
    /// </summary>
    public ShapeAlterationBuilder OnCreating(Func<ShapeCreatingContext, Task> actionAsync)
    {
        return Configure(descriptor =>
        {
            descriptor.CreatingAsync = [.. descriptor.CreatingAsync ?? [], actionAsync];
        });
    }

    /// <summary>
    /// Called when the shape is created.
    /// </summary>
    public ShapeAlterationBuilder OnCreated(Action<ShapeCreatedContext> action)
    {
        return OnCreated(ctx =>
        {
            action(ctx);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Called when the shape is being created.
    /// </summary>
    public ShapeAlterationBuilder OnCreated(Func<ShapeCreatedContext, Task> actionAsync)
    {
        return Configure(descriptor =>
        {
            descriptor.CreatedAsync = [.. descriptor.CreatedAsync ?? [], actionAsync];
        });
    }

    /// <summary>
    /// Called whenever the shape is displayed, even if it's content is cached.
    /// </summary>
    public ShapeAlterationBuilder OnDisplaying(Action<ShapeDisplayContext> action)
    {
        return OnDisplaying(ctx =>
        {
            action(ctx);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Called whenever the shape is displayed, even if it's content is cached.
    /// </summary>
    public ShapeAlterationBuilder OnDisplaying(Func<ShapeDisplayContext, Task> actionAsync)
    {
        return Configure(descriptor =>
        {
            descriptor.DisplayingAsync = [.. descriptor.DisplayingAsync ?? [], actionAsync];
        });
    }

    /// <summary>
    /// Called when the shape is actually rendered and state needs to be loaded.
    /// </summary>
    public ShapeAlterationBuilder OnProcessing(Action<ShapeDisplayContext> action)
    {
        return OnProcessing(ctx =>
        {
            action(ctx);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Called when the shape is actually rendered and state needs to be loaded.
    /// </summary>
    public ShapeAlterationBuilder OnProcessing(Func<ShapeDisplayContext, Task> actionAsync)
    {
        return Configure(descriptor =>
        {
            descriptor.ProcessingAsync = [.. descriptor.ProcessingAsync ?? [], actionAsync];
        });
    }

    /// <summary>
    /// Called when the shape is done being rendered.
    /// </summary>
    public ShapeAlterationBuilder OnDisplayed(Action<ShapeDisplayContext> action)
    {
        return OnDisplayed(ctx =>
        {
            action(ctx);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Called when the shape is done being rendered.
    /// </summary>
    public ShapeAlterationBuilder OnDisplayed(Func<ShapeDisplayContext, Task> actionAsync)
    {
        return Configure(descriptor =>
        {
            descriptor.DisplayedAsync = [.. descriptor.DisplayedAsync ?? [], actionAsync];
        });
    }

    public ShapeAlterationBuilder Placement(Func<ShapePlacementContext, PlacementInfo> action)
    {
        return Configure(descriptor =>
        {
            var next = descriptor.Placement;
            descriptor.Placement = ctx => action(ctx) ?? next(ctx);
        });
    }

    public ShapeAlterationBuilder Placement(Func<ShapePlacementContext, bool> predicate, PlacementInfo location)
    {
        return Configure(descriptor =>
        {
            var next = descriptor.Placement;
            descriptor.Placement = ctx => predicate(ctx) ? location : next(ctx);
        });
    }

    public ShapeAlteration Build()
    {
        return new ShapeAlteration(_shapeType, _feature, _configurations.ToArray());
    }
}

public class ShapePlacementContext
{
    public ShapePlacementContext(string shapeType, string displayType, string differentiator, IShape zoneShape)
    {
        ShapeType = shapeType;
        DisplayType = displayType;
        Differentiator = differentiator;
        ZoneShape = zoneShape;
    }

    public IShape ZoneShape { get; set; }
    public string ShapeType { get; set; }
    public string DisplayType { get; set; }
    public string Differentiator { get; set; }

    /// <summary>
    /// Debug information explaining where the final placement is coming from.
    /// Used by tooling.
    /// </summary>
    public string Source { get; set; }
}
