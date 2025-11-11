namespace OrchardCore.DisplayManagement;

/// <summary>
/// Provides a compile-time optimized way to convert an object's properties to <see cref="INamedEnumerable{T}"/>.
/// </summary>
/// <remarks>
/// Implement this interface to avoid reflection-based property access in <see cref="Arguments.From{T}(T)"/>.
/// This can be implemented manually or through source generation for optimal performance.
/// </remarks>
public interface IArgumentsProvider
{
    /// <summary>
    /// Gets the arguments from this object's properties.
    /// </summary>
    /// <returns>An enumerable of named arguments.</returns>
    INamedEnumerable<object> GetArguments();
}
