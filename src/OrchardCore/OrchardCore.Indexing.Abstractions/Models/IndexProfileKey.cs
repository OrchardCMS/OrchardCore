using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Indexing.Models;

public readonly record struct IndexProfileKey
{
    [SetsRequiredMembers]
    public IndexProfileKey(string providerName, string type)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(type);

        ProviderName = providerName;
        Type = type;
    }

    public required string ProviderName { get; init; }

    public required string Type { get; init; }
}
