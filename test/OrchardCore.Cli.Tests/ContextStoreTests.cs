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
            Contexts = [CreateContext($"tenant-{index}")],
        }, CancellationToken.None)));

        var configuration = await stores[0].LoadAsync(CancellationToken.None);
        Assert.StartsWith("tenant-", configuration.CurrentContext, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveAsync_ConcurrentRunsAddContexts_KeepsEveryContext()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(SaveAsync_ConcurrentRunsAddContexts_KeepsEveryContext)));

        // Every run loads the same state before any of them saves, like parallel provisioning.
        var stores = Enumerable.Range(0, 16).Select(_ => new ContextStore(paths)).ToArray();
        var configurations = await Task.WhenAll(stores.Select(store => store.LoadAsync(cancellationToken)));
        await Task.WhenAll(stores.Select((store, index) => Task.Run(async () =>
        {
            configurations[index].Contexts.Add(CreateContext($"tenant-{index}"));
            await store.SaveAsync(configurations[index], cancellationToken);
        }, cancellationToken)));

        var saved = await new ContextStore(paths).LoadAsync(cancellationToken);
        Assert.Equal(
            Enumerable.Range(0, 16).Select(index => $"tenant-{index}").Order(StringComparer.Ordinal),
            saved.Contexts.Select(context => context.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task SaveAsync_OtherRunAddedContext_KeepsItAndExposesIt()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(SaveAsync_OtherRunAddedContext_KeepsItAndExposesIt)));
        var (first, second) = (new ContextStore(paths), new ContextStore(paths));
        var (firstConfiguration, secondConfiguration) = (await first.LoadAsync(cancellationToken), await second.LoadAsync(cancellationToken));

        firstConfiguration.Contexts.Add(CreateContext("provisioned"));
        await first.SaveAsync(firstConfiguration, cancellationToken);
        secondConfiguration.CurrentContext = "other";
        secondConfiguration.Contexts.Add(CreateContext("other"));
        await second.SaveAsync(secondConfiguration, cancellationToken);

        Assert.Contains(secondConfiguration.Contexts, context => context.Name == "provisioned");
        var saved = await new ContextStore(paths).LoadAsync(cancellationToken);
        Assert.Equal(["other", "provisioned"], saved.Contexts.Select(context => context.Name).Order(StringComparer.Ordinal));
        Assert.Equal("other", saved.CurrentContext);
    }

    [Fact]
    public async Task SaveAsync_DeletedHere_DoesNotComeBack()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(SaveAsync_DeletedHere_DoesNotComeBack)));
        await SeedAsync(paths, "removed", "kept");
        var store = new ContextStore(paths);
        var configuration = await store.LoadAsync(cancellationToken);

        Assert.True(ContextStore.Delete(configuration, "removed"));
        await store.SaveAsync(configuration, cancellationToken);

        var saved = await new ContextStore(paths).LoadAsync(cancellationToken);
        Assert.Equal("kept", Assert.Single(saved.Contexts).Name);
        Assert.Equal("kept", saved.CurrentContext);
    }

    [Fact]
    public async Task SaveAsync_DeletedByOtherRun_StaysDeletedAndMovesCurrentContext()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(SaveAsync_DeletedByOtherRun_StaysDeletedAndMovesCurrentContext)));
        await SeedAsync(paths, "removed", "kept");
        var (deleting, refreshing) = (new ContextStore(paths), new ContextStore(paths));
        var (deletingConfiguration, refreshingConfiguration) = (await deleting.LoadAsync(cancellationToken), await refreshing.LoadAsync(cancellationToken));

        Assert.True(ContextStore.Delete(deletingConfiguration, "removed"));
        await deleting.SaveAsync(deletingConfiguration, cancellationToken);
        refreshingConfiguration.Contexts[0].ProductVersion = "4.0.0";
        await refreshing.SaveAsync(refreshingConfiguration, cancellationToken);

        var saved = await new ContextStore(paths).LoadAsync(cancellationToken);
        Assert.Equal("kept", Assert.Single(saved.Contexts).Name);
        Assert.Equal("kept", saved.CurrentContext);
    }

    [Fact]
    public async Task SaveAsync_ChangedByOtherRunOnly_KeepsTheirChange()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(SaveAsync_ChangedByOtherRunOnly_KeepsTheirChange)));
        await SeedAsync(paths, "first", "second");
        var (changing, stale) = (new ContextStore(paths), new ContextStore(paths));
        var (changingConfiguration, staleConfiguration) = (await changing.LoadAsync(cancellationToken), await stale.LoadAsync(cancellationToken));

        changingConfiguration.Contexts[0].ProductVersion = "4.0.0";
        changingConfiguration.CurrentContext = "second";
        await changing.SaveAsync(changingConfiguration, cancellationToken);
        staleConfiguration.Contexts[1].ProductVersion = "5.0.0";
        await stale.SaveAsync(staleConfiguration, cancellationToken);

        var saved = await new ContextStore(paths).LoadAsync(cancellationToken);
        Assert.Equal("4.0.0", saved.Contexts.Single(context => context.Name == "first").ProductVersion);
        Assert.Equal("5.0.0", saved.Contexts.Single(context => context.Name == "second").ProductVersion);
        Assert.Equal("second", saved.CurrentContext);
    }

    [Fact]
    public async Task UpdateAsync_UpdateThrows_LeavesFileUnchangedAndReleasesLock()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(UpdateAsync_UpdateThrows_LeavesFileUnchangedAndReleasesLock)));
        await SeedAsync(paths, "existing");
        var store = new ContextStore(paths);
        var configuration = await store.LoadAsync(cancellationToken);

        await Assert.ThrowsAsync<CliException>(() => store.UpdateAsync(configuration, updated =>
        {
            updated.Contexts.Clear();
            throw new CliException("Failed.");
        }, cancellationToken));

        Assert.Equal("existing", Assert.Single((await new ContextStore(paths).LoadAsync(cancellationToken)).Contexts).Name);
        await new ContextStore(paths).SaveAsync(new CliConfiguration(), cancellationToken);
    }

    private static async Task SeedAsync(CliPaths paths, params string[] names)
    {
        var store = new ContextStore(paths);
        var configuration = await store.LoadAsync(TestContext.Current.CancellationToken);
        configuration.CurrentContext = names[0];
        configuration.Contexts.AddRange(names.Select(CreateContext));
        await store.SaveAsync(configuration, TestContext.Current.CancellationToken);
    }

    private static TenantContextRecord CreateContext(string name) => new()
    {
        Name = name,
        TenantUrl = $"https://example.com/{name}/",
    };

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
