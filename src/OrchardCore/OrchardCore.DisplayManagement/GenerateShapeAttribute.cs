namespace OrchardCore.DisplayManagement;

/// <summary>
/// Marks a model type to generate a compile-time <see cref="IShape"/> implementation.
/// </summary>
/// <remarks>
/// Apply this attribute to a <c>partial</c> class with an accessible parameterless constructor.
/// The source generator will implement <see cref="IShape"/> and <see cref="IPositioned"/> directly on the model type,
/// which avoids both interceptors and runtime proxy generation for that model.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GenerateShapeAttribute : Attribute
{
}
