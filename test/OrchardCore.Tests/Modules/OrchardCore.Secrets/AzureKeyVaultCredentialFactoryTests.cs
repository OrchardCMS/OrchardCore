using Azure.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Secrets.Azure;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class AzureKeyVaultCredentialFactoryTests
{
    [Theory]
    [InlineData(AzureKeyVaultCredentialType.ManagedIdentity, typeof(ManagedIdentityCredential))]
    [InlineData(AzureKeyVaultCredentialType.WorkloadIdentity, typeof(WorkloadIdentityCredential))]
    [InlineData(AzureKeyVaultCredentialType.ClientSecret, typeof(ClientSecretCredential))]
    [InlineData(AzureKeyVaultCredentialType.AzureCli, typeof(AzureCliCredential))]
    [InlineData(AzureKeyVaultCredentialType.AzurePowerShell, typeof(AzurePowerShellCredential))]
    [InlineData(AzureKeyVaultCredentialType.VisualStudio, typeof(VisualStudioCredential))]
    [InlineData(AzureKeyVaultCredentialType.DefaultAzureCredential, typeof(DefaultAzureCredential))]
    public void Create_UsesOnlyExplicitlySelectedProvider(AzureKeyVaultCredentialType type, Type expected)
    {
        var options = new AzureKeyVaultSecretStoreOptions
        {
            CredentialType = type,
            TenantId = "tenant-id",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            TokenFilePath = "/var/run/secrets/azure/tokens/azure-identity-token",
        };

        var credential = AzureKeyVaultCredentialFactory.Create(options);

        Assert.IsType(expected, credential);
    }

    [Fact]
    public void Create_SystemAssignedManagedIdentity_DoesNotRequireApplicationCredentials()
    {
        var credential = AzureKeyVaultCredentialFactory.Create(new AzureKeyVaultSecretStoreOptions
        {
            CredentialType = AzureKeyVaultCredentialType.ManagedIdentity,
        });

        Assert.IsType<ManagedIdentityCredential>(credential);
    }

    [Theory]
    [InlineData(null)]
    [InlineData((AzureKeyVaultCredentialType)999)]
    public void Create_MissingOrUnsupportedSelection_DoesNotFallBack(AzureKeyVaultCredentialType? type)
    {
        var exception = Assert.Throws<OptionsValidationException>(() =>
            AzureKeyVaultCredentialFactory.Create(new AzureKeyVaultSecretStoreOptions
            {
                CredentialType = type,
                TenantId = "tenant-id",
                ClientId = "client-id",
                ClientSecret = "client-secret",
            }));

        Assert.Contains("CredentialType", exception.Message);
    }

    [Theory]
    [InlineData(AzureKeyVaultCredentialType.ClientSecret, "TenantId")]
    [InlineData(AzureKeyVaultCredentialType.ClientSecret, "ClientId")]
    [InlineData(AzureKeyVaultCredentialType.ClientSecret, "ClientSecret")]
    [InlineData(AzureKeyVaultCredentialType.WorkloadIdentity, "TenantId")]
    [InlineData(AzureKeyVaultCredentialType.WorkloadIdentity, "ClientId")]
    [InlineData(AzureKeyVaultCredentialType.WorkloadIdentity, "TokenFilePath")]
    [InlineData(AzureKeyVaultCredentialType.ManagedIdentity, "ClientId")]
    public void Create_MissingRequiredSetting_FailsExplicitly(AzureKeyVaultCredentialType type, string setting)
    {
        var options = new AzureKeyVaultSecretStoreOptions
        {
            CredentialType = type,
            TenantId = "tenant-id",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            TokenFilePath = "/var/run/secrets/azure/tokens/azure-identity-token",
        };
        switch (setting)
        {
            case "TenantId":
                options.TenantId = " ";
                break;
            case "ClientId":
                options.ClientId = " ";
                break;
            case "ClientSecret":
                options.ClientSecret = " ";
                break;
            case "TokenFilePath":
                options.TokenFilePath = " ";
                break;
        }

        var exception = Assert.Throws<OptionsValidationException>(() => AzureKeyVaultCredentialFactory.Create(options));

        Assert.Contains(setting, exception.Message);
        Assert.DoesNotContain("client-secret", exception.Message);
    }
}
