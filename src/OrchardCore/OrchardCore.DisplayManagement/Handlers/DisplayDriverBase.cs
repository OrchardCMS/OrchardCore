using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DisplayManagement.Handlers;

public class DisplayDriverBase
{
    protected string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new strongly typed shape.
    /// </summary>
    public ShapeResult Initialize<TModel>() where TModel : class
        => Initialize(typeof(TModel).Name, (Func<TModel, object, ValueTask>)null, null);

    /// <summary>
    /// Creates a new strongly typed shape.
    /// </summary>
    public ShapeResult Initialize<TModel>(string shapeType) where TModel : class
        => Initialize(shapeType, (Func<TModel, object, ValueTask>)null, null);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Initialize<TModel>(Action<TModel> initialize) where TModel : class
        => Initialize<TModel, Action<TModel>>(
            typeof(TModel).Name,
            static (shape, initialize) =>
            {
                initialize?.Invoke(shape);
                return ValueTask.CompletedTask;
            }, initialize);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Initialize<TModel>(string shapeType, Action<TModel> initialize) where TModel : class
        => Initialize<TModel, Action<TModel>>(
            shapeType,
            static (shape, initialize) =>
            {
                initialize?.Invoke(shape);
                return ValueTask.CompletedTask;
            },
            initialize);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState">The type of the state to pass to the initializer.</typeparam>
    /// <param name="initialize">A delegate that initializes the shape using the provided state.</param>
    /// <param name="state">The state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState>(Action<TModel, TState> initialize, TState state) where TModel : class
        => Initialize<TModel, (Action<TModel, TState>, TState)>(
            typeof(TModel).Name,
            static (shape, state) =>
            {
                var (initialize, initializeState) = state;
                initialize?.Invoke(shape, initializeState);
                return ValueTask.CompletedTask;
            },
            (initialize, state));

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState">The type of the state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initialize">A delegate that initializes the shape using the provided state.</param>
    /// <param name="state">The state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState>(string shapeType, Action<TModel, TState> initialize, TState state) where TModel : class
        => Initialize<TModel, (Action<TModel, TState>, TState)>(
            shapeType,
            static (shape, state) =>
            {
                var (initialize, initializeState) = state;
                initialize?.Invoke(shape, initializeState);
                return ValueTask.CompletedTask;
            },
            (initialize, state));

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Initialize<TModel>(Func<TModel, ValueTask> initializeAsync) where TModel : class
        => Initialize<TModel, Func<TModel, ValueTask>>(
            typeof(TModel).Name,
            static (shape, initializeAsync) => initializeAsync != null ? initializeAsync.Invoke(shape) : ValueTask.CompletedTask,
            initializeAsync);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Initialize<TModel>(string shapeType, Func<TModel, ValueTask> initializeAsync) where TModel : class
       => Initialize<TModel, Func<TModel, ValueTask>>(
           shapeType,
           static (shape, initializeAsync) => initializeAsync != null ? initializeAsync.Invoke(shape) : ValueTask.CompletedTask,
           initializeAsync);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState">The type of the state to pass to the initializer.</typeparam>
    /// <param name="initializeAsync">A delegate that initializes the shape asynchronously using the provided state.</param>
    /// <param name="state">The state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState>(Func<TModel, TState, ValueTask> initializeAsync, TState state) where TModel : class
        => Initialize(typeof(TModel).Name, initializeAsync, state);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState">The type of the state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initializeAsync">A delegate that initializes the shape asynchronously using the provided state.</param>
    /// <param name="state">The state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState>(string shapeType, Func<TModel, TState, ValueTask> initializeAsync, TState state) where TModel : class
        => Factory(
            shapeType,
            static (context, state) =>
            {
                var (shapeType, initializeAsync, initializeState) = state;
                return context.ShapeFactory.CreateAsync(shapeType, initializeAsync, initializeState);
            },
            (shapeType, initializeAsync, state),
            null,
            (object)null);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// Supports multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <param name="initialize">A delegate that initializes the shape using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2>(Action<TModel, TState1, TState2> initialize, TState1 state1, TState2 state2) where TModel : class
        => Initialize(typeof(TModel).Name, initialize, state1, state2);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// Supports multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initialize">A delegate that initializes the shape using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2>(string shapeType, Action<TModel, TState1, TState2> initialize, TState1 state1, TState2 state2) where TModel : class
    {
        return Initialize<TModel, (Action<TModel, TState1, TState2>, TState1, TState2)>(
            shapeType,
            static (shape, state) =>
            {
                var (initialize, state1, state2) = state;
                initialize?.Invoke(shape, state1, state2);
            },
            (initialize, state1, state2));
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// Supports multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <param name="initialize">A delegate that initializes the shape using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3>(Action<TModel, TState1, TState2, TState3> initialize, TState1 state1, TState2 state2, TState3 state3) where TModel : class
        => Initialize(typeof(TModel).Name, initialize, state1, state2, state3);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// Supports multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initialize">A delegate that initializes the shape using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3>(string shapeType, Action<TModel, TState1, TState2, TState3> initialize, TState1 state1, TState2 state2, TState3 state3) where TModel : class
    {
        return Initialize<TModel, (Action<TModel, TState1, TState2, TState3>, TState1, TState2, TState3)>(
            shapeType,
            static (shape, state) =>
            {
                var (initialize, state1, state2, state3) = state;
                initialize?.Invoke(shape, state1, state2, state3);
            },
            (initialize, state1, state2, state3));
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// Supports multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <typeparam name="TState4">The type of the fourth state to pass to the initializer.</typeparam>
    /// <param name="initialize">A delegate that initializes the shape using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    /// <param name="state4">The fourth state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3, TState4>(Action<TModel, TState1, TState2, TState3, TState4> initialize, TState1 state1, TState2 state2, TState3 state3, TState4 state4) where TModel : class
        => Initialize(typeof(TModel).Name, initialize, state1, state2, state3, state4);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// Supports multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <typeparam name="TState4">The type of the fourth state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initialize">A delegate that initializes the shape using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    /// <param name="state4">The fourth state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3, TState4>(string shapeType, Action<TModel, TState1, TState2, TState3, TState4> initialize, TState1 state1, TState2 state2, TState3 state3, TState4 state4) where TModel : class
    {
        return Initialize<TModel, (Action<TModel, TState1, TState2, TState3, TState4>, TState1, TState2, TState3, TState4)>(
            shapeType,
            static (shape, state) =>
            {
                var (initialize, state1, state2, state3, state4) = state;
                initialize?.Invoke(shape, state1, state2, state3, state4);
            },
            (initialize, state1, state2, state3, state4));
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// Supports multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <typeparam name="TState4">The type of the fourth state to pass to the initializer.</typeparam>
    /// <typeparam name="TState5">The type of the fifth state to pass to the initializer.</typeparam>
    /// <param name="initialize">A delegate that initializes the shape using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    /// <param name="state4">The fourth state to pass to the initializer delegate.</param>
    /// <param name="state5"> The fifth state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3, TState4, TState5>(Action<TModel, TState1, TState2, TState3, TState4, TState5> initialize, TState1 state1, TState2 state2, TState3 state3, TState4 state4, TState5 state5) where TModel : class
        => Initialize(typeof(TModel).Name, initialize, state1, state2, state3, state4, state5);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it using the provided state to avoid closures.
    /// Supports multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <typeparam name="TState4">The type of the fourth state to pass to the initializer.</typeparam>
    /// <typeparam name="TState5">The type of the fifth state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initialize">A delegate that initializes the shape using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    /// <param name="state4">The fourth state to pass to the initializer delegate.</param>
    /// <param name="state5"> The fifth state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3, TState4, TState5>(string shapeType, Action<TModel, TState1, TState2, TState3, TState4, TState5> initialize, TState1 state1, TState2 state2, TState3 state3, TState4 state4, TState5 state5) where TModel : class
    {
        return Initialize<TModel, (Action<TModel, TState1, TState2, TState3, TState4, TState5>, TState1, TState2, TState3, TState4, TState5)>(
            shapeType,
            static (shape, state) =>
            {
                var (initialize, state1, state2, state3, state4, state5) = state;
                initialize?.Invoke(shape, state1, state2, state3, state4, state5);
            },
            (initialize, state1, state2, state3, state4, state5));
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// Supports asynchronous initialization with multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <param name="initializeAsync">A delegate that initializes the shape asynchronously using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2>(Func<TModel, TState1, TState2, ValueTask> initializeAsync, TState1 state1, TState2 state2) where TModel : class
        => Initialize(typeof(TModel).Name, initializeAsync, state1, state2);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// Supports asynchronous initialization with multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initializeAsync">A delegate that initializes the shape asynchronously using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2>(string shapeType, Func<TModel, TState1, TState2, ValueTask> initializeAsync, TState1 state1, TState2 state2) where TModel : class
    {
        return Initialize<TModel, (Func<TModel, TState1, TState2, ValueTask>, TState1, TState2)>(
            shapeType,
            static (shape, state) =>
            {
                var (initializeAsync, state1, state2) = state;
                return initializeAsync?.Invoke(shape, state1, state2) ?? ValueTask.CompletedTask;
            },
            (initializeAsync, state1, state2));
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// Supports asynchronous initialization with multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <param name="initializeAsync">A delegate that initializes the shape asynchronously using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3>(Func<TModel, TState1, TState2, TState3, ValueTask> initializeAsync, TState1 state1, TState2 state2, TState3 state3) where TModel : class
        => Initialize(typeof(TModel).Name, initializeAsync, state1, state2, state3);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// Supports asynchronous initialization with multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initializeAsync">A delegate that initializes the shape asynchronously using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3>(string shapeType, Func<TModel, TState1, TState2, TState3, ValueTask> initializeAsync, TState1 state1, TState2 state2, TState3 state3) where TModel : class
    {
        return Initialize<TModel, (Func<TModel, TState1, TState2, TState3, ValueTask>, TState1, TState2, TState3)>(
            shapeType,
            static (shape, state) =>
            {
                var (initializeAsync, state1, state2, state3) = state;
                return initializeAsync?.Invoke(shape, state1, state2, state3) ?? ValueTask.CompletedTask;
            },
            (initializeAsync, state1, state2, state3));
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// Supports asynchronous initialization with multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <typeparam name="TState4">The type of the fourth state to pass to the initializer.</typeparam>
    /// <param name="initializeAsync">A delegate that initializes the shape asynchronously using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    /// <param name="state4">The fourth state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3, TState4>(Func<TModel, TState1, TState2, TState3, TState4, ValueTask> initializeAsync, TState1 state1, TState2 state2, TState3 state3, TState4 state4) where TModel : class
        => Initialize(typeof(TModel).Name, initializeAsync, state1, state2, state3, state4);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// Supports asynchronous initialization with multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <typeparam name="TState4">The type of the fourth state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initializeAsync">A delegate that initializes the shape asynchronously using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    /// <param name="state4">The fourth state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3, TState4>(string shapeType, Func<TModel, TState1, TState2, TState3, TState4, ValueTask> initializeAsync, TState1 state1, TState2 state2, TState3 state3, TState4 state4) where TModel : class
    {
        return Initialize<TModel, (Func<TModel, TState1, TState2, TState3, TState4, ValueTask>, TState1, TState2, TState3, TState4)>(
            shapeType,
            static (shape, state) =>
            {
                var (initializeAsync, state1, state2, state3, state4) = state;
                return initializeAsync?.Invoke(shape, state1, state2, state3, state4) ?? ValueTask.CompletedTask;
            },
            (initializeAsync, state1, state2, state3, state4));
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// Supports asynchronous initialization with multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <typeparam name="TState4">The type of the fourth state to pass to the initializer.</typeparam>
    /// <typeparam name="TState5">The type of the fifth state to pass to the initializer.</typeparam>
    /// <param name="initializeAsync">A delegate that initializes the shape asynchronously using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    /// <param name="state4">The fourth state to pass to the initializer delegate.</param>
    /// <param name="state5"> The fifth state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3, TState4, TState5>(Func<TModel, TState1, TState2, TState3, TState4, TState5, ValueTask> initializeAsync, TState1 state1, TState2 state2, TState3 state3, TState4 state4, TState5 state5) where TModel : class
        => Initialize(typeof(TModel).Name, initializeAsync, state1, state2, state3, state4, state5);

    /// <summary>
    /// Creates a new strongly typed shape and initializes it before it is displayed.
    /// Supports asynchronous initialization with multiple state parameters using value tuples.
    /// </summary>
    /// <typeparam name="TModel">The type of the shape model.</typeparam>
    /// <typeparam name="TState1">The type of the first state to pass to the initializer.</typeparam>
    /// <typeparam name="TState2">The type of the second state to pass to the initializer.</typeparam>
    /// <typeparam name="TState3">The type of the third state to pass to the initializer.</typeparam>
    /// <typeparam name="TState4">The type of the fourth state to pass to the initializer.</typeparam>
    /// <typeparam name="TState5">The type of the fifth state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initializeAsync">A delegate that initializes the shape asynchronously using the provided states.</param>
    /// <param name="state1">The first state to pass to the initializer delegate.</param>
    /// <param name="state2">The second state to pass to the initializer delegate.</param>
    /// <param name="state3">The third state to pass to the initializer delegate.</param>
    /// <param name="state4">The fourth state to pass to the initializer delegate.</param>
    /// <param name="state5"> The fifth state to pass to the initializer delegate.</param>
    public ShapeResult Initialize<TModel, TState1, TState2, TState3, TState4, TState5>(string shapeType, Func<TModel, TState1, TState2, TState3, TState4, TState5, ValueTask> initializeAsync, TState1 state1, TState2 state2, TState3 state3, TState4 state4, TState5 state5) where TModel : class
    {
        return Initialize<TModel, (Func<TModel, TState1, TState2, TState3, TState4, TState5, ValueTask>, TState1, TState2, TState3, TState4, TState5)>(
            shapeType,
            static (shape, state) =>
            {
                var (initializeAsync, state1, state2, state3, state4, state5) = state;
                return initializeAsync?.Invoke(shape, state1, state2, state3, state4, state5) ?? ValueTask.CompletedTask;
            },
            (initializeAsync, state1, state2, state3, state4, state5));
    }

    /// <summary>
    /// Creates a new strongly typed shape and initializes its properties from an existing object.
    /// </summary>
    public ShapeResult Copy<TModel>(string shapeType, TModel model) where TModel : class
        => Factory(
            shapeType,
            static (context, state) => context.ShapeFactory.CreateAsync(state.shapeType, state.model),
            (shapeType, model),
            null,
            (object)null);

    /// <summary>
    /// When the shape is displayed, it is created automatically from its type name.
    /// </summary>
    public ShapeResult Dynamic(string shapeType)
        => Factory(
            shapeType,
            static (context, shapeTypeState) => context.ShapeFactory.CreateAsync(shapeTypeState),
            shapeType,
            null,
            (object)null);

    /// <summary>
    /// Creates a new loosely typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Dynamic(string shapeType, Action<dynamic> initialize)
        => Factory(
            shapeType,
            static (context, shapeTypeState) => context.ShapeFactory.CreateAsync(shapeTypeState),
            shapeType,
            static (shape, initialize) =>
            {
                initialize?.Invoke(shape);
                return ValueTask.CompletedTask;
            },
            initialize);

    /// <summary>
    /// Creates a new loosely typed shape and initializes it using state to avoid closures.
    /// </summary>
    /// <typeparam name="TState">The type of the state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initialize">A delegate that initializes the shape using the provided state.</param>
    /// <param name="state">The state to pass to the initializer delegate.</param>
    public ShapeResult Dynamic<TState>(string shapeType, Action<dynamic, TState> initialize, TState state)
        => Factory(
            shapeType,
            static (context, shapeTypeState) => context.ShapeFactory.CreateAsync(shapeTypeState),
            shapeType,
            static (shape, state) =>
            {
                var (initialize, initializeState) = state;
                initialize?.Invoke(shape, initializeState);
                return ValueTask.CompletedTask;
            },
            (initialize, state));

    /// <summary>
    /// Creates a new loosely typed shape and initializes it before it is displayed.
    /// </summary>
    public ShapeResult Dynamic(string shapeType, Func<dynamic, ValueTask> initializeAsync)
        => Factory(
            shapeType,
            static (context, shapeTypeState) => context.ShapeFactory.CreateAsync(shapeTypeState),
            shapeType,
            static (shape, initializeAsync) => initializeAsync?.Invoke(shape) ?? ValueTask.CompletedTask,
            initializeAsync
        );

    /// <summary>
    /// Creates a new loosely typed shape and initializes it using state to avoid closures.
    /// </summary>
    /// <typeparam name="TState">The type of the state to pass to the initializer.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="initializeAsync">A delegate that initializes the shape using the provided state.</param>
    /// <param name="state">The state to pass to the initializer delegate.</param>
    public ShapeResult Dynamic<TState>(string shapeType, Func<dynamic, TState, ValueTask> initializeAsync, TState state)
        => Factory(
            shapeType,
            static (context, shapeTypeState) => context.ShapeFactory.CreateAsync(shapeTypeState),
            shapeType,
            static (shape, state) =>
            {
                var (initializeAsync, initializeState) = state;
                return initializeAsync?.Invoke(shape, initializeState) ?? ValueTask.CompletedTask;
            },
            (initializeAsync, state)
        );

    /// <summary>
    /// Creates a <see cref="ShapeViewModel{TModel}"/> for the specific model.
    /// </summary>
    public ShapeResult View<TModel>(string shapeType, TModel model) where TModel : class
        => Factory(
            shapeType,
            static (context, model) => ValueTask.FromResult<IShape>(new ShapeViewModel<TModel>(model)),
            model,
            null,
            (object)null);

    /// <summary>
    /// If the shape needs to be rendered, it is created automatically from its type name and initialized.
    /// </summary>
    [Obsolete("This method has been deprecated, because of bad performance characteristics. Use one of the Factory() methods instead.")]
    public ShapeResult Shape(string shapeType, IShape shape)
        => Factory(shapeType, static (context, shape) => ValueTask.FromResult(shape), shape, null, (object)null);

    /// <summary>
    /// Creates a shape lazily.
    /// </summary>
    public ShapeResult Factory(string shapeType, Func<IShape> shapeBuilder)
        => Factory(shapeType, static (context, shapeBuilder) => ValueTask.FromResult(shapeBuilder()), shapeBuilder, null, (object)null);

    /// <summary>
    /// Creates a shape lazily using state to avoid closures.
    /// </summary>
    /// <typeparam name="TState">The type of the state to pass to the shape builder.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="shapeBuilder">A delegate that creates the shape using the provided state.</param>
    /// <param name="state">The state to pass to the shape builder delegate.</param>
    public ShapeResult Factory<TState>(string shapeType, Func<TState, IShape> shapeBuilder, TState state)
        => Factory(shapeType, static (context, state) => ValueTask.FromResult(state.shapeBuilder(state.state)), (shapeBuilder, state), null, (object)null);

    /// <summary>
    /// Creates a shape lazily.
    /// </summary>
    public ShapeResult Factory(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder)
        => Factory(shapeType, static (context, shapeBuilder) => shapeBuilder(context), shapeBuilder, null, (object)null);

    /// <summary>
    /// Creates a shape lazily using state to avoid closures.
    /// </summary>
    /// <typeparam name="TState">The type of the state to pass to the shape builder.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="shapeBuilder">A delegate that creates the shape using the provided state.</param>
    /// <param name="state">The state to pass to the shape builder delegate.</param>
    public ShapeResult Factory<TState>(string shapeType, Func<IBuildShapeContext, TState, ValueTask<IShape>> shapeBuilder, TState state)
        => Factory(shapeType, shapeBuilder, state, null, (object)null);

    /// <summary>
    /// Creates a shape lazily.
    /// </summary>
    public ShapeResult Factory(string shapeType, Func<IBuildShapeContext, IShape> shapeBuilder)
        => Factory(shapeType, static (context, shapeBuilder) => ValueTask.FromResult(shapeBuilder(context)), shapeBuilder, null, (object)null);

    /// <summary>
    /// Creates a shape lazily using state to avoid closures.
    /// </summary>
    /// <typeparam name="TState">The type of the state to pass to the shape builder.</typeparam>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="shapeBuilder">A delegate that creates the shape using the provided state.</param>
    /// <param name="state">The state to pass to the shape builder delegate.</param>
    public ShapeResult Factory<TState>(string shapeType, Func<IBuildShapeContext, TState, IShape> shapeBuilder, TState state)
        => Factory(
            shapeType,
            static (context, state) => ValueTask.FromResult(state.shapeBuilder(context, state.state)),
            (shapeBuilder, state),
            null,
            (object)null);

    /// <summary>
    /// If the shape needs to be displayed, it is created by the delegate.
    /// </summary>
    public ShapeResult Factory(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder, Func<IShape, ValueTask> initializeAsync)
        => Factory(
            shapeType,
            static (context, shapeBuilder) => shapeBuilder(context),
            shapeBuilder,
            static (shape, initializeAsync) => initializeAsync?.Invoke(shape) ?? ValueTask.CompletedTask,
             initializeAsync);

    /// <summary>
    /// If the shape needs to be displayed, it is created by the delegate using state to avoid closures.
    /// </summary>
    /// <param name="shapeType">The shape type name.</param>
    /// <param name="shapeBuilder">A delegate that creates the shape using the provided state.</param>
    /// <param name="shapeBuilderState">The state to pass to the shape builder delegate.</param>
    /// <param name="initializeAsync">A delegate that initializes the shape using the provided state.</param>
    /// <param name="initializeState">The state to pass to the initializer delegate.</param>
    /// <remarks>
    /// This method is ultimately called by all drivers to create a shape. It's made virtual
    /// so that any concrete driver can use it as a way to alter any returning shape from the drivers.
    /// </remarks>
    public virtual ShapeResult Factory<TBuilderState, TInitState>(
        string shapeType,
        Func<IBuildShapeContext, TBuilderState, ValueTask<IShape>> shapeBuilder,
        TBuilderState shapeBuilderState,
        Func<IShape, TInitState, ValueTask> initializeAsync,
        TInitState initializeState)
        => ShapeResult.Create(
            shapeType,
            shapeBuilder,
            shapeBuilderState,
            initializeAsync,
            initializeState
        ).Prefix(Prefix);

    public static CombinedResult Combine(params IDisplayResult[] results)
        => new(results);

    public static Task<IDisplayResult> CombineAsync(params IDisplayResult[] results)
        => Task.FromResult<IDisplayResult>(new CombinedResult(results));

    public static CombinedResult Combine(IEnumerable<IDisplayResult> results)
        => new(results);

    public static Task<IDisplayResult> CombineAsync(IEnumerable<IDisplayResult> results)
        => Task.FromResult<IDisplayResult>(new CombinedResult(results));
}
