namespace OrchardCore.DisplayManagement;

/// <summary>
/// Marks a type to generate an <see cref="IArgumentsProvider"/> implementation via source generation.
/// </summary>
/// <remarks>
/// When applied to a type, a source generator can create an optimized implementation of
/// <see cref="IArgumentsProvider"/> that avoids reflection-based property access.
/// Until a source generator is available, types can manually implement <see cref="IArgumentsProvider"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class GenerateArgumentsProviderAttribute : Attribute
{
}
