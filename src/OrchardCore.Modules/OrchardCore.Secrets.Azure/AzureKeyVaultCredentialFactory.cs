using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace OrchardCore.Secrets.Azure;

public static class AzureKeyVaultCredentialFactory
{
    public static TokenCredential Create(AzureKeyVaultSecretStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.CredentialType switch
        {
            AzureKeyVaultCredentialType.ManagedIdentity => new ManagedIdentityCredential(
                string.IsNullOrEmpty(options.ClientId)
                    ? ManagedIdentityId.SystemAssigned
                    : ManagedIdentityId.FromUserAssignedClientId(Require(options.ClientId, nameof(options.ClientId)))),
            AzureKeyVaultCredentialType.WorkloadIdentity => new WorkloadIdentityCredential(new WorkloadIdentityCredentialOptions
            {
                TenantId = Require(options.TenantId, nameof(options.TenantId)),
                ClientId = Require(options.ClientId, nameof(options.ClientId)),
                TokenFilePath = Require(options.TokenFilePath, nameof(options.TokenFilePath)),
            }),
            AzureKeyVaultCredentialType.ClientSecret => new ClientSecretCredential(
                Require(options.TenantId, nameof(options.TenantId)),
                Require(options.ClientId, nameof(options.ClientId)),
                Require(options.ClientSecret, nameof(options.ClientSecret))),
            AzureKeyVaultCredentialType.AzureCli => new AzureCliCredential(),
            AzureKeyVaultCredentialType.AzurePowerShell => new AzurePowerShellCredential(),
            AzureKeyVaultCredentialType.VisualStudio => new VisualStudioCredential(),
            AzureKeyVaultCredentialType.DefaultAzureCredential => new DefaultAzureCredential(),
            _ => throw InvalidOption(nameof(options.CredentialType), "must explicitly select a supported credential provider"),
        };
    }

    private static string Require(string value, string name) =>
        !string.IsNullOrWhiteSpace(value)
            ? value
            : throw InvalidOption(name, "is required for the selected credential provider");

    private static OptionsValidationException InvalidOption(string name, string reason) =>
        new(Options.DefaultName, typeof(AzureKeyVaultSecretStoreOptions), [$"'OrchardCore:Secrets:Azure:{name}' {reason}."]);
}
