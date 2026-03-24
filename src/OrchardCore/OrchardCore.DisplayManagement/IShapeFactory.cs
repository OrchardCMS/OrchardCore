using OrchardCore.DisplayManagement.Implementation;

namespace OrchardCore.DisplayManagement;

/// <summary>
/// Service that creates new instances of dynamic shape objects.
/// This may be used directly, or through the <c>IShapeHelperFactory</c>.
/// </summary>
public interface IShapeFactory
{
    /// <summary>
    /// Creates a new shape instance of the specified type.
    /// </summary>
    /// <param name="shapeType">The type name of the shape to create.</param>
    /// <param name="shapeFactory">A factory function that produces the initial <see cref="IShape"/> instance.</param>
    /// <param name="creating">A callback invoked before the shape is created, allowing modification of the <see cref="ShapeCreatingContext"/>.</param>
    /// <param name="created">A callback invoked after the shape is created.</param>
    /// <returns>A <see cref="ValueTask{IShape}"/> that resolves to the created shape.</returns>
    /// <remarks>
    /// If the callbacks or factory need to capture external variables, prefer using
    /// <see cref="CreateAsync{TState}(string, Func{TState, ValueTask{IShape}}, Action{ShapeCreatingContext, TState}, Action{ShapeCreatedContext, TState}, TState)"/>
    /// to avoid closure allocations.
    /// </remarks>
    ValueTask<IShape> CreateAsync(
        string shapeType,
        Func<ValueTask<IShape>> shapeFactory,
        Action<ShapeCreatingContext> creating,
        Action<ShapeCreatedContext> created);

    /// <summary>
    /// Creates a new shape instance of the specified type, passing a <typeparamref name="TState"/> value
    /// through the callbacks and factory to avoid closure allocations.
    /// </summary>
    /// <typeparam name="TState">The type of the state object passed to the factory and callbacks.</typeparam>
    /// <param name="shapeType">The type name of the shape to create.</param>
    /// <param name="shapeFactory">A factory function that receives <paramref name="state"/> and produces the initial <see cref="IShape"/> instance.</param>
    /// <param name="creating">A callback invoked before the shape is created, receiving both the <see cref="ShapeCreatingContext"/> and <paramref name="state"/>.</param>
    /// <param name="created">A callback invoked after the shape is created, receiving both the <see cref="ShapeCreatedContext"/> and <paramref name="state"/>.</param>
    /// <param name="state">A state object passed to <paramref name="shapeFactory"/>, <paramref name="creating"/>, and <paramref name="created"/> to avoid closure allocations.</param>
    /// <returns>A <see cref="ValueTask{IShape}"/> that resolves to the created shape.</returns>
    ValueTask<IShape> CreateAsync<TState>(
        string shapeType,
        Func<TState, ValueTask<IShape>> shapeFactory,
        Action<ShapeCreatingContext, TState> creating,
        Action<ShapeCreatedContext, TState> created,
        TState state)
        => CreateAsync(
            shapeType,
            () => shapeFactory(state),
            creating is null ? null : (ctx) => creating(ctx, state),
            created is null ? null : (ctx) => created(ctx, state));

    /// <summary>
    /// Gets a dynamic proxy object that provides a fluent API for creating shapes by name.
    /// </summary>
    /// <value>A dynamic object whose members correspond to shape type names.</value>
    dynamic New { get; }
}
