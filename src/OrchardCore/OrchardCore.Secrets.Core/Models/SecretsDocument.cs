using OrchardCore.Data.Documents;

namespace OrchardCore.Secrets.Models;

/// <summary>
/// Stores secret entries in the database.
/// </summary>
public sealed class SecretsDocument : Document
{
    /// <summary>
    /// Gets the collection of secret entries keyed by their name.
    /// </summary>
    public Dictionary<string, SecretEntry> Secrets { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
