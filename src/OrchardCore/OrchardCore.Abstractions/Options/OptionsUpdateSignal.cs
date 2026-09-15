namespace OrchardCore.Environment.Options;

/// <summary>
/// Builds the distributed signal key used to invalidate a specific options type and name.
/// </summary>
public static class OptionsUpdateSignal
{
    private const string Prefix = "OrchardCore.Environment.Options.Update:";

    /// <summary>
    /// Gets the distributed signal key for the specified options type and name.
    /// </summary>
    /// <param name="optionsType">The options type to invalidate.</param>
    /// <param name="name">The named options instance to invalidate.</param>
    public static string GetKey(Type optionsType, string name)
    {
        ArgumentNullException.ThrowIfNull(optionsType);

        name ??= Microsoft.Extensions.Options.Options.DefaultName;

        var typeName = optionsType.AssemblyQualifiedName ?? optionsType.FullName ?? optionsType.Name;

        return $"{Prefix}{typeName.Length}:{typeName}:{name.Length}:{name}";
    }
}
