namespace OrchardCore.Localization;

/// <summary>
/// Represents a default implementation for providing a culture aliases.
/// </summary>
public class DefaultCultureAliasProvider : ICultureAliasProvider
{
    /// <inheritdoc/>
    public string[] GetAliases() => new[] { "zh-CN", "zh-TW" }; 
}
