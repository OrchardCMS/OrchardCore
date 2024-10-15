using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DisplayManagement.Handlers;

public class DisplayDriverBase
{
    protected string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new strongly typed shape.
    /// </summary>
    public ShapeResult Initialize<TModel>() where TModel : class
    {
        return Initialize<TModel>(shape => { });
    }

    /// <summary>
    /// Creates a new strongly typed shape.
    /// </summary>
    public ShapeResult Initialize<TModel>(string shapeType) where TModel : class
    {
        return Initialize<TModel>(shapeType, shape => { });
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Initialize<TModel>(Action<TModel> initialize) where TModel : class
    {
        return Initialize<TModel>(typeof(TModel).Name, initialize);
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Initialize<TModel>(string shapeType, Action<TModel> initialize) where TModel : class
    {
        return Initialize<TModel>(shapeType, shape =>
        {
            initialize?.Invoke(shape);

            return ValueTask.CompletedTask;
        });
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Initialize<TModel>(Func<TModel, ValueTask> initializeAsync) where TModel : class
    {
        return Initialize(
            typeof(TModel).Name,
            initializeAsync
            );
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Initialize<TModel>(string shapeType, Func<TModel, ValueTask> initializeAsync) where TModel : class
    {
        return Factory(
            shapeType,
            shapeBuilder: ctx => ctx.ShapeFactory.CreateAsync<TModel>(shapeType),
            initializeAsync: shape => initializeAsync?.Invoke((TModel)shape).AsTask()
            );
    }

    /// <summary>
    /// Creates a new strongly typed shape an initializes its properties from an existing object.
    /// </summary>
    public ShapeResult Copy<TModel>(string shapeType, TModel model) where TModel : class
    {
        return Factory(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType, model));
    }

    /// <summary>
    /// Creates a new loosely typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Dynamic(string shapeType, Func<dynamic, Task> initializeAsync)
    {
        return Factory(
            shapeType,
            ctx => ctx.ShapeFactory.CreateAsync(shapeType),
            initializeAsync
        );
    }

    /// <summary>
    /// Creates a new loosely typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Dynamic(string shapeType, Action<dynamic> initialize)
    {
        return Dynamic(
            shapeType,
            initializeAsync: shape =>
            {
                initialize?.Invoke(shape);
                return Task.FromResult(shape);
            }
        );
    }

    /// <summary>
    /// When the shape is displayed, it is created automatically from its type name.
    /// </summary>
    public ShapeResult Dynamic(string shapeType)
    {
        return Dynamic(shapeType, shape => Task.CompletedTask);
    }

    /// <summary>
    /// Creates a <see cref="ShapeViewModel{TModel}"/> for the specific model.
    /// </summary>
    public ShapeResult View<TModel>(string shapeType, TModel model) where TModel : class
    {
        return Factory(shapeType, ctx => ValueTask.FromResult<IShape>(new ShapeViewModel<TModel>(model)));
    }

    /// <summary>
    /// If the shape needs to be rendered, it is created automatically from its type name and initialized.
    /// </summary>
    public ShapeResult Shape(string shapeType, IShape shape)
    {
        return Factory(shapeType, ctx => ValueTask.FromResult<IShape>(shape));
    }

    /// <summary>
    /// Creates a shape lazily.
    /// </summary>
    public ShapeResult Factory(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder)
    {
        return Factory(shapeType, shapeBuilder, null);
    }

    /// <summary>
    /// Creates a shape lazily.
    /// </summary>
    public ShapeResult Factory(string shapeType, Func<IBuildShapeContext, IShape> shapeBuilder)
    {
        return Factory(shapeType, ctx => ValueTask.FromResult<IShape>(shapeBuilder(ctx)), null);
    }

    /// <summary>
    /// If the shape needs to be displayed, it is created by the delegate.
    /// </summary>
    /// <remarks>
    /// This method is ultimately called by all drivers to create a shape. It's made virtual
    /// so that any concrete driver can use it as a way to alter any returning shape from the drivers.
    /// </remarks>
    public virtual ShapeResult Factory(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder, Func<IShape, Task> initializeAsync)
    {
        return new ShapeResult(shapeType, shapeBuilder, initializeAsync)
            .Prefix(Prefix);
    }

    public static CombinedResult Combine(params IDisplayResult[] results)
        => new(results);

    public static Task<IDisplayResult> CombineAsync(params IDisplayResult[] results)
        => Task.FromResult<IDisplayResult>(new CombinedResult(results));

    public static CombinedResult Combine(IEnumerable<IDisplayResult> results)
        => new(results);

    public static Task<IDisplayResult> CombineAsync(IEnumerable<IDisplayResult> results)
        => Task.FromResult<IDisplayResult>(new CombinedResult(results));
}
