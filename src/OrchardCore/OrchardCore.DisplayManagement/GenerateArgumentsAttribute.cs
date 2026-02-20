namespace OrchardCore.DisplayManagement;

/// <summary>
/// Marks a type to generate an <see cref="INamedEnumerable{T}"/> implementation via source generation.
/// </summary>
/// <remarks>
/// When applied to a type, a source generator creates an optimized implementation of
/// <see cref="INamedEnumerable{T}"/> that avoids reflection-based property access and array allocations.
/// The generated class directly accesses properties on-demand without creating intermediate arrays.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class GenerateArgumentsAttribute : Attribute
{
}
