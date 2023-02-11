namespace OrchardCore.Localization;

/// <summary>
/// Contract for provding a culture aliases.
/// </summary>
public interface ICultureAliasProvider
{
    /// <summary>
    /// Gets a culture alias(es).
    /// </summary>
    string[] GetAliases();
}
