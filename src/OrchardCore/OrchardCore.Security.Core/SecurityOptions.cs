using Microsoft.Extensions.Localization;

namespace OrchardCore.Security.Core;

public sealed class SecurityOptions
{
    public IReadOnlyDictionary<string, CredentialOptionsEntry> CredentialProviders
        => _credentialProviders;

    private readonly Dictionary<string, CredentialOptionsEntry> _credentialProviders = new(StringComparer.OrdinalIgnoreCase);

    public void AddCredentialsProvider(string providerName, Action<CredentialOptionsEntry> configure = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);

        if (!_credentialProviders.TryGetValue(providerName, out var entry))
        {
            entry = new CredentialOptionsEntry(providerName);
        }

        if (configure != null)
        {
            configure(entry);
        }

        if (string.IsNullOrEmpty(entry.DisplayName))
        {
            entry.DisplayName = new LocalizedString(providerName, providerName);
        }

        _credentialProviders[providerName] = entry;
    }
}

public sealed class CredentialOptionsEntry
{
    public CredentialOptionsEntry(string providerName)
    {
        ProviderName = providerName;
    }

    public string ProviderName { get; }

    public LocalizedString DisplayName { get; set; }

    public LocalizedString Description { get; set; }
}
