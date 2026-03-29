using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DisplayManagement;

public static class ShapeFactoryExtensions
{
    private static readonly ConcurrentDictionary<Type, Type> _generatedShapeTypes = [];

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
        => factory.CreateAsync<object>(shapeType, static (state) => ValueTask.FromResult<IShape>(new Shape()), null, null, null);

    /// <summary>
    /// Creates a new generic shape instance and initializes it.
    /// </summary>
    public static ValueTask<IShape> CreateAsync(this IShapeFactory factory, string shapeType, Func<ValueTask<IShape>> shapeFactory)
        => factory.CreateAsync(shapeType, shapeFactory, null, null);

    /// <summary>
    /// Creates a new generic shape instance and initializes it.
    /// </summary>
    public static ValueTask<IShape> CreateAsync<TState>(this IShapeFactory factory, string shapeType, Func<TState, ValueTask<IShape>> shapeFactory, TState state)
        => factory.CreateAsync(shapeType, shapeFactory, null, null, state);

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
        return factory.CreateAsync<TModel, Action<TModel>>(shapeType, initializeAsync: static (model, initialize) =>
        {
            initialize?.Invoke(model);

            return ValueTask.CompletedTask;
        }, initialize);
    }

    public static ValueTask<IShape> CreateAsync<TModel, TState>(this IShapeFactory factory, string shapeType, Action<TModel, TState> initialize, TState state)
        where TModel : class
    {
        return factory.CreateAsync<TModel, (Action<TModel, TState>, TState)>(shapeType, initializeAsync: static (model, state) =>
        {
            var (initialize, arg) = state;
            initialize?.Invoke(model, arg);

            return ValueTask.CompletedTask;
        }, (initialize, state));
    }

    /// <summary>
    /// Creates a generic shape and initializes it with some properties.
    /// </summary>
    public static ValueTask<IShape> CreateAsync(this IShapeFactory factory, string shapeType, INamedEnumerable<object> parameters)
    {
        ArgumentException.ThrowIfNullOrEmpty(shapeType);

        if (parameters == null || parameters.Count == 0)
        {
            return factory.CreateAsync(shapeType);
        }

        return factory.CreateAsync(shapeType, static (state) => ValueTask.FromResult<IShape>(new Shape()), null, static (createdContext, parameters) =>
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
        }, parameters);
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
        => factory.CreateAsync<TModel, Func<TModel, ValueTask>>(shapeType, static (model, initializeAsync) => initializeAsync is null ? ValueTask.CompletedTask : initializeAsync(model), initializeAsync);

    public static ValueTask<IShape> CreateAsync<TModel, TState>(this IShapeFactory factory, string shapeType, Func<TModel, TState, ValueTask> initializeAsync, TState state)
        where TModel : class
    {
        ArgumentException.ThrowIfNullOrEmpty(shapeType);

        return factory.CreateAsync(shapeType, static (state) => ShapeFactory(state.initializeAsync, state.state), (initializeAsync, state));

        static ValueTask<IShape> ShapeFactory(Func<TModel, TState, ValueTask> init, TState state)
        {
            var shape = CreateStronglyTypedShape(typeof(TModel));

            if (init != null)
            {
                var task = init((TModel)shape, state);

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

    internal static void RegisterGeneratedShapeType(Type baseType, Type generatedShapeType)
    {
        ArgumentNullException.ThrowIfNull(baseType);
        ArgumentNullException.ThrowIfNull(generatedShapeType);

        if (!baseType.IsAssignableFrom(generatedShapeType))
        {
            throw new ArgumentException($"'{generatedShapeType}' must inherit from '{baseType}'.", nameof(generatedShapeType));
        }

        if (!typeof(IShape).IsAssignableFrom(generatedShapeType))
        {
            throw new ArgumentException($"'{generatedShapeType}' must implement '{typeof(IShape)}'.", nameof(generatedShapeType));
        }

        _generatedShapeTypes.TryAdd(baseType, generatedShapeType);
    }

    internal static bool TryGetGeneratedShapeType(Type baseType, out Type generatedShapeType)
        => _generatedShapeTypes.TryGetValue(baseType, out generatedShapeType);

    /// <summary>
    /// Creates a source-generated shape instance for the specified type.
    /// </summary>
    /// <param name="baseType">The type of the new shape to create.</param>
    /// <returns>
    /// A new shape instance implementing <see cref="IShape"/> and inheriting from the type <paramref name="baseType"/>.
    /// </returns>
    /// <remarks>
    /// If <paramref name="baseType"/> implements <see cref="IShape"/> then no dynamic proxy type is used.
    /// </remarks>
    private static IShape CreateStronglyTypedShape(Type baseType)
    {
        // Don't generate a shape wrapper for shape types.
        if (typeof(IShape).IsAssignableFrom(baseType))
        {
            return (IShape)Activator.CreateInstance(baseType);
        }

        if (TryGetGeneratedShapeType(baseType, out var generatedShapeType))
        {
            return (IShape)Activator.CreateInstance(generatedShapeType);
        }

        throw new InvalidOperationException($"No generated shape type was registered for '{baseType.FullName}'. Ensure the assembly using '{baseType.Name}' is built with OrchardCore source generators.");
    }
}

public static class GeneratedShapeTypeRegistry
{
    public static void Register(Type baseType, Type generatedShapeType)
        => ShapeFactoryExtensions.RegisterGeneratedShapeType(baseType, generatedShapeType);

    public static bool TryGetGeneratedShapeType(Type baseType, out Type generatedShapeType)
        => ShapeFactoryExtensions.TryGetGeneratedShapeType(baseType, out generatedShapeType);
}
