using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Azure;
using OrchardCore.Secrets.Providers;
using ISecret = OrchardCore.Secrets.ISecret;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class AzureKeyVaultSecretStoreTests
{
    private readonly Dictionary<string, KeyVaultSecret> _entries = [];
    private readonly Mock<SecretClient> _client = new();

    public AzureKeyVaultSecretStoreTests()
    {
        _client.Setup(c => c.GetSecretAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .Returns((string name, string version, CancellationToken cancellationToken) =>
                _entries.TryGetValue(name, out var entry)
                    ? Task.FromResult(Response.FromValue(entry, Mock.Of<Response>()))
                    : Task.FromException<Response<KeyVaultSecret>>(new RequestFailedException(404, "Not found")));
        _client.Setup(c => c.SetSecretAsync(It.IsAny<KeyVaultSecret>(), It.IsAny<CancellationToken>()))
            .Returns((KeyVaultSecret entry, CancellationToken cancellationToken) =>
            {
                _entries[entry.Name] = entry;
                return Task.FromResult(Response.FromValue(entry, Mock.Of<Response>()));
            });
        _client.Setup(c => c.GetPropertiesOfSecretsAsync(It.IsAny<CancellationToken>()))
            .Returns(() => AsyncPageable<SecretProperties>.FromPages(
                [Page<SecretProperties>.FromValues(_entries.Values.Select(e => e.Properties).ToList(), null, Mock.Of<Response>())]));
    }

    private AzureKeyVaultSecretStore CreateStore(string tenant) =>
        new(_client.Object, tenant,
            [new TextSecretTypeProvider(Mock.Of<IStringLocalizer<TextSecretTypeProvider>>()),
                new RsaKeySecretTypeProvider(Mock.Of<IStringLocalizer<RsaKeySecretTypeProvider>>()),
                new X509SecretTypeProvider(Mock.Of<IStringLocalizer<X509SecretTypeProvider>>())],
            NullLogger<AzureKeyVaultSecretStore>.Instance);

    [Fact]
    public async Task SharedVault_IsolatesTenantReadsWritesAndEnumeration()
    {
        var first = CreateStore("First");
        var second = CreateStore("Second");
        await first.SaveSecretAsync("ApiKey", new TextSecret { Text = "first" });

        Assert.Null(await second.GetSecretAsync<TextSecret>("ApiKey"));
        Assert.Empty(await second.GetSecretInfosAsync());
        await second.RemoveSecretAsync("ApiKey");
        _client.Verify(c => c.StartDeleteSecretAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        await second.SaveSecretAsync("ApiKey", new TextSecret { Text = "second" });
        Assert.Equal("first", (await first.GetSecretAsync<TextSecret>("ApiKey")).Text);
        Assert.Equal("second", (await second.GetSecretAsync<TextSecret>("ApiKey")).Text);
        Assert.Single(await first.GetSecretInfosAsync());
        Assert.Single(await second.GetSecretInfosAsync());
        Assert.Equal(2, _entries.Count);
    }

    [Fact]
    public async Task Names_DoNotCollideAfterEncoding()
    {
        var store = CreateStore("tenant");
        string[] names = ["Payment.ApiKey", "Payment_ApiKey", "Payment:ApiKey", "Payment-ApiKey", "payment-apikey", new string('x', 256)];
        foreach (var name in names)
        {
            await store.SaveSecretAsync(name, new TextSecret { Text = name });
        }

        Assert.Equal(names.Length, _entries.Count);
        foreach (var name in names)
        {
            Assert.Equal(name, (await store.GetSecretAsync<TextSecret>(name)).Text);
        }

        Assert.All(_entries.Keys, name =>
        {
            Assert.InRange(name.Length, 1, 127);
            Assert.Matches("^[a-z0-9-]+$", name);
        });
    }

    [Fact]
    public async Task RetrievalAndListing_PreserveTypeAndMetadata()
    {
        var store = CreateStore("tenant");
        var expires = DateTime.UtcNow.AddDays(30);
        await store.SaveSecretAsync("rsa", new RsaKeySecret { PublicKey = "public" },
            new SecretSaveOptions { Description = "description", ExpiresUtc = expires });

        Assert.IsType<RsaKeySecret>(await store.GetSecretAsync<ISecret>("rsa"));
        Assert.Null(await store.GetSecretAsync<TextSecret>("rsa"));
        var info = Assert.Single(await store.GetSecretInfosAsync());
        Assert.Equal("rsa", info.Name);
        Assert.Equal(nameof(RsaKeySecret), info.Type);
        Assert.Equal("description", info.Description);
        Assert.Equal(expires, info.ExpiresUtc);

        await store.SaveSecretAsync("rsa", new RsaKeySecret { PublicKey = "updated" });
        info = Assert.Single(await store.GetSecretInfosAsync());
        Assert.Equal("description", info.Description);
        Assert.Equal(expires, info.ExpiresUtc);

        await store.SaveSecretAsync("rsa", new RsaKeySecret { PublicKey = "updated" }, new SecretSaveOptions());
        info = Assert.Single(await store.GetSecretInfosAsync());
        Assert.Null(info.Description);
        Assert.Null(info.ExpiresUtc);
    }

    [Fact]
    public async Task UntaggedOrForeignEntries_AreNotAccessible()
    {
        var store = CreateStore("tenant");
        _entries["legacy"] = new KeyVaultSecret("legacy", "legacy value");
        Assert.Empty(await store.GetSecretInfosAsync());
        Assert.Null(await store.GetSecretAsync<TextSecret>("legacy"));

        await store.SaveSecretAsync("owned", new TextSecret { Text = "value" });
        var entry = _entries.Values.Single(e => e.Name != "legacy");
        entry.Properties.Tags["OrchardTenant"] = "foreign";
        Assert.Empty(await store.GetSecretInfosAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.GetSecretAsync<ISecret>("owned"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.SaveSecretAsync("owned", new TextSecret()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.RemoveSecretAsync("owned"));
    }

    [Fact]
    public async Task Delete_DoesNotPurge()
    {
        var store = CreateStore("tenant");
        await store.SaveSecretAsync("secret", new TextSecret { Text = "value" });
        var name = Assert.Single(_entries.Keys);
        var deleted = SecretModelFactory.DeletedSecret(new SecretProperties(name), "value");
        var operation = new Mock<DeleteSecretOperation>();
        operation.Setup(o => o.WaitForCompletionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(deleted, Mock.Of<Response>()));
        _client.Setup(c => c.StartDeleteSecretAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(operation.Object);

        await store.RemoveSecretAsync("secret");

        _client.Verify(c => c.StartDeleteSecretAsync(name, It.IsAny<CancellationToken>()), Times.Once);
        _client.Verify(c => c.PurgeDeletedSecretAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VaultFailure_IsNotReportedAsMissingSecret()
    {
        _client.Setup(c => c.GetSecretAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));
        await Assert.ThrowsAsync<RequestFailedException>(() => CreateStore("tenant").GetSecretAsync<ISecret>("secret"));
    }
}
