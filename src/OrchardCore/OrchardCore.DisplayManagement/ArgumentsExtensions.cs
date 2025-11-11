namespace OrchardCore.DisplayManagement;

/// <summary>
/// Extension methods for creating <see cref="INamedEnumerable{T}"/> from objects.
/// </summary>
public static class ArgumentsExtensions
{
    /// <summary>
    /// Converts a dictionary to arguments.
    /// </summary>
    public static INamedEnumerable<object> ToArguments(this IDictionary<string, object> dictionary)
        => Arguments.From(dictionary);

    /// <summary>
    /// Converts a string dictionary to arguments.
    /// </summary>
    public static INamedEnumerable<string> ToArguments(this IDictionary<string, string> dictionary)
        => Arguments.From(dictionary);

    /// <summary>
    /// Converts an object's properties to arguments.
    /// For best performance, the type should implement <see cref="IArgumentsProvider"/>.
    /// </summary>
    public static INamedEnumerable<object> ToArguments<T>(this T obj) where T : notnull
        => Arguments.From(obj);
}
