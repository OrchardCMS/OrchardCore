using System.Collections.Concurrent;
using Castle.DynamicProxy;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DisplayManagement;

public static class ShapeFactoryExtensions
{
    private static readonly ConcurrentDictionary<Type, Type> _proxyTypesCache = [];
    private static readonly ProxyGenerator _proxyGenerator = new();
    private static readonly Func<ValueTask<IShape>> _newShape = () => ValueTask.FromResult<IShape>(new Shape());

    /// <summary>
    /// Creates a new generic shape by copying the properties of an object.
    /// </summary>
    /// <param name="factory">The <see cref="IShapeFactory"/>.</param>
    /// <param name="shapeType">The type of shape to create.</param>
    /// <param name="model">The model to copy.</param>
    /// <returns></returns>
    public static ValueTask<IShape> CreateAsync<TModel>(this IShapeFactory factory, string shapeType, TModel model)
        => factory.CreateAsync(shapeType, Arguments.From(model));

    /// <summary>
    /// Creates a new generic shape instance.
    /// </summary>
    public static ValueTask<IShape> CreateAsync(this IShapeFactory factory, string shapeType)
        => factory.CreateAsync(shapeType, _newShape);

    /// <summary>
    /// Creates a new generic shape instance and initializes it.
    /// </summary>
    public static ValueTask<IShape> CreateAsync(this IShapeFactory factory, string shapeType, Func<ValueTask<IShape>> shapeFactory)
        => factory.CreateAsync(shapeType, shapeFactory, null, null);

    /// <summary>
    /// Creates a dynamic proxy instance for the type and initializes it.
    /// </summary>
    /// <typeparam name="TModel">The type to instantiate.</typeparam>
    /// <param name="factory">The <see cref="IShapeFactory"/>.</param>
    /// <param name="initialize">The initialization method.</param>
    /// <returns></returns>
    public static ValueTask<IShape> CreateAsync<TModel>(this IShapeFactory factory, Action<TModel> initialize = null)
        where TModel : class
        => factory.CreateAsync(typeof(TModel).Name, initialize);

    /// <summary>
    /// Creates a dynamic proxy instance for the type and initializes it.
    /// </summary>
    /// <typeparam name="TModel">The type to instantiate.</typeparam>
    /// <param name="factory">The <see cref="IShapeFactory"/>.</param>
    /// <param name="shapeType">The shape type to create.</param>
    /// <param name="initialize">The initialization method.</param>
    /// <returns></returns>
    public static ValueTask<IShape> CreateAsync<TModel>(this IShapeFactory factory, string shapeType, Action<TModel> initialize = null)
        where TModel : class
    {
        return factory.CreateAsync<TModel>(shapeType, initializeAsync: (model) =>
        {
            initialize?.Invoke(model);

            return ValueTask.CompletedTask;
        });
    }

    /// <summary>
    /// Creates a generic shape and initializes it with some properties.
    /// </summary>
    public static ValueTask<IShape> CreateAsync(this IShapeFactory factory, string shapeType, INamedEnumerable<object> parameters)
    {
        ArgumentException.ThrowIfNullOrEmpty(shapeType);

        if (parameters == null || parameters == Arguments.Empty)
        {
            return factory.CreateAsync(shapeType);
        }

        return factory.CreateAsync(shapeType, _newShape, null, createdContext =>
        {
            var shape = (Shape)createdContext.Shape;

            // If only one non-Type, use it as the source object to copy.
            var initializer = parameters.Positional.SingleOrDefault();

            if (initializer != null)
            {
                // Use the Arguments class to optimize reflection code.
                var arguments = Arguments.From(initializer);

                foreach (var prop in arguments.Named)
                {
                    shape.Properties[prop.Key] = prop.Value;
                }
            }
            else
            {
                foreach (var kv in parameters.Named)
                {
                    shape.Properties[kv.Key] = kv.Value;
                }
            }
        });
    }

    /// <summary>
    /// Creates a dynamic proxy instance for the type and initializes it.
    /// </summary>
    /// <typeparam name="TModel">The type to instantiate.</typeparam>
    /// <param name="factory">The <see cref="IShapeFactory"/>.</param>
    /// <param name="initializeAsync">The initialization method.</param>
    /// <returns></returns>
    public static ValueTask<IShape> CreateAsync<TModel>(this IShapeFactory factory, Func<TModel, ValueTask> initializeAsync)
        where TModel : class
        => factory.CreateAsync(typeof(TModel).Name, initializeAsync);

    /// <summary>
    /// Creates a dynamic proxy instance for the type and initializes it.
    /// </summary>
    /// <typeparam name="TModel">The type to instantiate.</typeparam>
    /// <param name="factory">The <see cref="IShapeFactory"/>.</param>
    /// <param name="shapeType">The shape type to create.</param>
    /// <param name="initializeAsync">The initialization method.</param>
    /// <returns></returns>
    public static ValueTask<IShape> CreateAsync<TModel>(this IShapeFactory factory, string shapeType, Func<TModel, ValueTask> initializeAsync)
        where TModel : class
    {
        ArgumentException.ThrowIfNullOrEmpty(shapeType);

        return factory.CreateAsync(shapeType, () => ShapeFactory(initializeAsync));

        static ValueTask<IShape> ShapeFactory(Func<TModel, ValueTask> init)
        {
            var shape = CreateStronglyTypedShape(typeof(TModel));

            if (init != null)
            {
                var task = init((TModel)shape);

                if (!task.IsCompletedSuccessfully)
                {
                    return Awaited(task, shape);
                }
            }

            return ValueTask.FromResult(shape);
        }

        static async ValueTask<IShape> Awaited(ValueTask task, IShape shape)
        {
            await task;

            return shape;
        }
    }

    /// <summary>
    /// Creates a dynamic proxy based shape instance for the specified type.
    /// </summary>
    /// <param name="baseType">The type of the new shape to create.</param>
    /// <returns>
    /// A new shape instance implementing <see cref="IShape"/> and inheriting from the type <paramref name="baseType"/>.
    /// </returns>
    /// <remarks>
    /// If <paramref name="baseType"/> implements <see cref="IShape"/> then no dynamic proxy type is used.
    /// </remarks>
    internal static IShape CreateStronglyTypedShape(Type baseType)
    {
        var shapeType = baseType;

        // Don't generate a proxy for shape types.
        if (typeof(IShape).IsAssignableFrom(shapeType))
        {
            return (IShape)Activator.CreateInstance(baseType);
        }

        if (_proxyTypesCache.TryGetValue(baseType, out var proxyType))
        {
            var model = new ShapeViewModel();

            return (IShape)Activator.CreateInstance(proxyType, model, model, Array.Empty<IInterceptor>());
        }

        var options = new ProxyGenerationOptions();
        options.AddMixinInstance(new ShapeViewModel());
        var shape = (IShape)_proxyGenerator.CreateClassProxy(baseType, options);

        _proxyTypesCache.TryAdd(baseType, shape.GetType());

        return shape;
    }
}
