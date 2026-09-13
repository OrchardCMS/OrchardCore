using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli.Tests;

public class ContextStoreTests
{
    [Fact]
    public async Task SaveAsync_LoadAsync_RoundTripsContexts()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(SaveAsync_LoadAsync_RoundTripsContexts)));
        var store = new ContextStore(paths);
        var configuration = new CliConfiguration
        {
            CurrentContext = "primary",
            Contexts =
            [
                new TenantContextRecord
                {
                    Name = "primary",
                    TenantUrl = "https://example.com/tenant/",
                    ManagementManifestUrl = "https://example.com/tenant/api/management/manifest",
                },
            ],
        };

        await store.SaveAsync(configuration, CancellationToken.None);
        var loaded = await store.LoadAsync(CancellationToken.None);

        Assert.Equal("primary", loaded.CurrentContext);
        Assert.Single(loaded.Contexts);
        Assert.Equal("https://example.com/tenant/", loaded.Contexts[0].TenantUrl);
    }

    [Fact]
    public async Task SaveAsync_ConcurrentWriters_DoNotShareTheDestinationFile()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(SaveAsync_ConcurrentWriters_DoNotShareTheDestinationFile)));
        var stores = Enumerable.Range(0, 10).Select(_ => new ContextStore(paths)).ToArray();

        await Task.WhenAll(stores.Select((store, index) => store.SaveAsync(new CliConfiguration
        {
            CurrentContext = $"tenant-{index}",
        }, CancellationToken.None)));

        var configuration = await stores[0].LoadAsync(CancellationToken.None);
        Assert.StartsWith("tenant-", configuration.CurrentContext, StringComparison.Ordinal);
    }

    [Fact]
    public void AddOrUpdate_WhenManifestIsProvided_StoresNormalizedTenantMetadata()
    {
        var configuration = new CliConfiguration();
        var manifest = new RemoteManagementManifest
        {
            ProductVersion = "3.0.0",
            ManagementManifestUrl = new Uri("https://example.com/tenant/api/management/manifest"),
            OpenApiUrl = new Uri("https://example.com/tenant/swagger/v1/swagger.json"),
            DocumentationIndexUrl = new Uri("https://docs.orchardcore.net/en/latest/search/search_index.json"),
        };

        manifest.Authentication.Authority = new Uri("https://example.com/tenant/");
        manifest.Authentication.ClientId = "orchardcore-cli";
        manifest.Authentication.GrantTypes.Add("device_code");
        manifest.Authentication.Scopes.Add("offline_access");

        var record = ContextStore.AddOrUpdate(configuration, "tenant", manifest, makeCurrent: true);

        Assert.Equal("tenant", configuration.CurrentContext);
        Assert.Equal("https://example.com/tenant/", record.TenantUrl);
        Assert.Equal("https://example.com/tenant/", record.Authority);
        Assert.Equal("device_code", Assert.Single(record.GrantTypes));
        Assert.Equal("offline_access", Assert.Single(record.Scopes));
    }
}
