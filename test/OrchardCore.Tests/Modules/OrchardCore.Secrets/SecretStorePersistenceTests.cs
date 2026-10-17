using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking.Distributed;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Services;
using OrchardCore.Tests.Apis.Context;
using ISecret = OrchardCore.Secrets.ISecret;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretStorePersistenceTests
{
    [Fact]
    public async Task BulkTransfer_RoundTripsRealDatabaseAndCleanupPersistsAcrossScopes()
    {
        using var context = new SiteContext().WithRecipe("SaaS");
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            await features.EnableFeaturesAsync((await features.GetAvailableFeaturesAsync())
                .Where(feature => feature.Id == "OrchardCore.Secrets"), force: true);
        });
        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        var expires = DateTime.UtcNow.AddDays(30);
        var memory = new SecretStoreOperationsTests.MemoryStore("TestStore");
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<ISecretManager>();
            await manager.SaveSecretAsync("text", new TextSecret { Text = "private-text" },
                new SecretSaveOptions { Description = "description", ExpiresUtc = expires });
            await manager.SaveSecretAsync("rsa", new RsaKeySecret { PublicKey = "public", PrivateKey = "private-key" });
            await manager.SaveSecretAsync("x509", new X509Secret { Thumbprint = "thumbprint" });
            expires = (await manager.GetSecretInfosAsync()).Single(info => info.Name == "text").ExpiresUtc.Value;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var operations = CreateOperations(scope.ServiceProvider, memory);
            var results = await operations.MoveAllSecretsAsync("Database", "TestStore");
            Assert.Equal(3, results.Count);
            Assert.All(results, result => Assert.True(result.Succeeded, result.Failure.ToString()));
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<ISecretManager>();
            Assert.Empty(await manager.GetSecretInfosAsync());
            Assert.Null(await manager.GetSecretAsync<ISecret>("text"));
            Assert.Equal("private-text", (await memory.GetSecretAsync<TextSecret>("text")).Text);
            Assert.Equal(expires, memory.Entries["text"].Info.ExpiresUtc);
            Assert.Equal("description", memory.Entries["text"].Info.Description);
            var results = await CreateOperations(scope.ServiceProvider, memory).MoveAllSecretsAsync("TestStore", "Database");
            Assert.Equal(3, results.Count);
            Assert.All(results, result => Assert.True(result.Succeeded, result.Failure.ToString()));
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<ISecretManager>();
            Assert.Equal("private-text", (await manager.GetSecretAsync<TextSecret>("text")).Text);
            Assert.Equal("private-key", (await manager.GetSecretAsync<RsaKeySecret>("rsa")).PrivateKey);
            Assert.Equal("thumbprint", (await manager.GetSecretAsync<X509Secret>("x509")).Thumbprint);
            var document = await scope.ServiceProvider.GetRequiredService<IDocumentManager<SecretsDocument>>().GetOrCreateImmutableAsync();
            Assert.Equal(3, document.Secrets.Count);
            Assert.All(document.Secrets.Values, entry =>
            {
                Assert.DoesNotContain("private-text", entry.EncryptedData);
                Assert.DoesNotContain("private-key", entry.EncryptedData);
            });
            foreach (var info in await manager.GetSecretInfosAsync())
            {
                await manager.RemoveSecretAsync(info.Name, "Database");
            }
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var document = await scope.ServiceProvider.GetRequiredService<IDocumentManager<SecretsDocument>>().GetOrCreateImmutableAsync();
            Assert.Empty(document.Secrets);
            Assert.Empty(await scope.ServiceProvider.GetRequiredService<ISecretManager>().GetSecretInfosAsync());
        });
    }

    private static SecretStoreOperations CreateOperations(IServiceProvider provider, ISecretStore otherStore)
    {
        var database = provider.GetRequiredService<ISecretManager>().GetStores().Single(store => store.Name == "Database");
        var locking = provider.GetRequiredService<IDistributedLock>();
        var manager = new SecretManager([database, otherStore], NullLogger<SecretManager>.Instance, locking);
        return new SecretStoreOperations(manager, provider.GetServices<ISecretTypeProvider>(), locking, NullLogger<SecretStoreOperations>.Instance);
    }
}
