using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Secrets.Azure;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class AzureKeyVaultSecretStoreOptionsSetupTests
{
    [Fact]
    public void Configure_ReadsHierarchicalSection()
    {
        var configuration = new ShellConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Secrets:Azure:AzureClient"] = "SharedVault",
                ["Secrets:Azure:NamePrefix"] = "my-application",
            }));
        var options = new AzureKeyVaultSecretStoreOptions();

        new AzureKeyVaultSecretStoreOptionsSetup(configuration).Configure(options);

        Assert.Equal("my-application", options.NamePrefix);
        Assert.Equal("SharedVault", options.AzureClient);
    }

    [Fact]
    public void Configure_UnspecifiedPrefix_PreservesDefault()
    {
        var options = new AzureKeyVaultSecretStoreOptions();

        new AzureKeyVaultSecretStoreOptionsSetup(new ShellConfiguration(new ConfigurationBuilder())).Configure(options);

        Assert.Equal("oc", options.NamePrefix);
        Assert.Null(options.AzureClient);
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
