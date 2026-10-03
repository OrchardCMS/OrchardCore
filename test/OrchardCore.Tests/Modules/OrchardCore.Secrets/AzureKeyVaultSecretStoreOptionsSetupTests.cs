using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Secrets.Azure;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class AzureKeyVaultSecretStoreOptionsSetupTests
{
    [Fact]
    public void Configure_ReadsHierarchicalSection()
    {
        const string section = "Secrets:Azure";
        var configuration = new ShellConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [$"{section}:VaultUri"] = "https://test.vault.azure.net/",
                [$"{section}:NamePrefix"] = "my-application",
                [$"{section}:CredentialType"] = "WorkloadIdentity",
                [$"{section}:TenantId"] = "tenant-id",
                [$"{section}:ClientId"] = "client-id",
                [$"{section}:ClientSecret"] = "client-secret",
                [$"{section}:TokenFilePath"] = "/var/run/secrets/azure/token",
            }));
        var options = new AzureKeyVaultSecretStoreOptions();

        new AzureKeyVaultSecretStoreOptionsSetup(configuration).Configure(options);

        Assert.Equal("https://test.vault.azure.net/", options.VaultUri);
        Assert.Equal("my-application", options.NamePrefix);
        Assert.Equal(AzureKeyVaultCredentialType.WorkloadIdentity, options.CredentialType);
        Assert.Equal("tenant-id", options.TenantId);
        Assert.Equal("client-id", options.ClientId);
        Assert.Equal("client-secret", options.ClientSecret);
        Assert.Equal("/var/run/secrets/azure/token", options.TokenFilePath);
    }

    [Fact]
    public void Configure_UnspecifiedPrefix_PreservesDefault()
    {
        var options = new AzureKeyVaultSecretStoreOptions();

        new AzureKeyVaultSecretStoreOptionsSetup(new ShellConfiguration(new ConfigurationBuilder())).Configure(options);

        Assert.Equal("oc", options.NamePrefix);
    }

    [Fact]
    public void Configure_BlankPrefix_DoesNotSilentlyUseDefault()
    {
        var configuration = new ShellConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Secrets:Azure:NamePrefix"] = " ",
            }));
        var options = new AzureKeyVaultSecretStoreOptions();

        new AzureKeyVaultSecretStoreOptionsSetup(configuration).Configure(options);

        Assert.Equal(" ", options.NamePrefix);
    }
}
