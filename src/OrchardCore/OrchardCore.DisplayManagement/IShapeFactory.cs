using OrchardCore.DisplayManagement.Implementation;

namespace OrchardCore.DisplayManagement;

/// <summary>
/// Service that creates new instances of dynamic shape objects
/// This may be used directly, or through the IShapeHelperFactory.
/// </summary>
public interface IShapeFactory
{
    ValueTask<IShape> CreateAsync(
        string shapeType,
        Func<ValueTask<IShape>> shapeFactory,
        Action<ShapeCreatingContext> creating,
        Action<ShapeCreatedContext> created);

    ValueTask<IShape> CreateAsync<TState>(
        string shapeType,
        Func<TState, ValueTask<IShape>> shapeFactory,
        Action<ShapeCreatingContext, TState> creating,
        Action<ShapeCreatedContext, TState> created,
        TState state) => CreateAsync(shapeType, () => shapeFactory(state), (ctx) => creating(ctx, state), (ctx) => created(ctx, state));

    dynamic New { get; }
}
