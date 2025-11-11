namespace OrchardCore.DisplayManagement;

/// <summary>
/// Helper methods for creating <see cref="IArgumentsProvider"/> implementations.
/// </summary>
/// <remarks>
/// This class provides utilities for manually implementing <see cref="IArgumentsProvider"/>
/// when source generation is not available or desired.
/// </remarks>
public static class ArgumentsProviderHelper
{
    /// <summary>
    /// Creates an <see cref="INamedEnumerable{T}"/> from arrays of values and names.
    /// </summary>
    /// <param name="values">The property values.</param>
    /// <param name="names">The property names.</param>
    /// <returns>A named enumerable containing the values.</returns>
    public static INamedEnumerable<object> Create(object[] values, string[] names)
    {
        return Arguments.From(values, names);
    }

    /// <summary>
    /// Creates an <see cref="INamedEnumerable{T}"/> from a dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary containing name-value pairs.</param>
    /// <returns>A named enumerable containing the values.</returns>
    public static INamedEnumerable<object> Create(IDictionary<string, object> dictionary)
    {
        return Arguments.From(dictionary);
    }
}
