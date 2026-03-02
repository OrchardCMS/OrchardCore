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

    ValueTask<IShape> CreateAsync(
        string shapeType,
        Func<object, ValueTask<IShape>> shapeFactory,
        Action<ShapeCreatingContext> creating,
        Action<ShapeCreatedContext> created,
        object state) => CreateAsync(shapeType, () => shapeFactory(state), creating, created);

    dynamic New { get; }
}
