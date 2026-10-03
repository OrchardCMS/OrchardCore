using System.Security.Cryptography;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Providers;
using OrchardCore.Secrets.Services;
using ISecret = OrchardCore.Secrets.ISecret;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretEncryptionServiceTests
{
    private readonly SecretEncryptionService _service;

    public SecretEncryptionServiceTests()
    {
        using var rsa = RSA.Create(2048);
        var key = new RsaKeySecret
        {
            PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
            PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
            IncludesPrivateKey = true,
        };
        var manager = new Mock<ISecretManager>();
        manager.Setup(m => m.GetSecretAsync<RsaKeySecret>("key")).ReturnsAsync(key);
        _service = new SecretEncryptionService(manager.Object,
            [new TextSecretTypeProvider(Mock.Of<IStringLocalizer<TextSecretTypeProvider>>()),
                new RsaKeySecretTypeProvider(Mock.Of<IStringLocalizer<RsaKeySecretTypeProvider>>()), new CustomSecretProvider()]);
    }

    [Fact]
    public async Task RoundTrip_PreservesRegisteredCustomType()
    {
        var info = new SecretInfo { Name = "custom", Store = "Database", Type = typeof(CustomSecret).FullName };
        var encrypted = await _service.EncryptAsync(new CustomSecret { Value = "custom value" }, "key", info);
        var decrypted = await _service.DecryptAsync(encrypted, "key", info);

        Assert.Equal("custom value", Assert.IsType<CustomSecret>(decrypted).Value);
        Assert.Equal(1, encrypted.Version);
        Assert.Equal(12, Convert.FromBase64String(encrypted.IV).Length);
        Assert.Equal(16, Convert.FromBase64String(encrypted.Tag).Length);
    }

    [Theory]
    [InlineData("ciphertext")]
    [InlineData("nonce")]
    [InlineData("tag")]
    [InlineData("key")]
    [InlineData("name")]
    [InlineData("store")]
    [InlineData("type")]
    [InlineData("description")]
    [InlineData("expiration")]
    public async Task DecryptAsync_RejectsTamperedPayloadAndMetadata(string field)
    {
        var info = new SecretInfo { Name = "secret", Store = "Database", Type = nameof(TextSecret) };
        var encrypted = await _service.EncryptAsync(new TextSecret { Text = "safe" }, "key", info);
        switch (field)
        {
            case "ciphertext": encrypted.EncryptedData = FlipByte(encrypted.EncryptedData); break;
            case "nonce": encrypted.IV = FlipByte(encrypted.IV); break;
            case "tag": encrypted.Tag = FlipByte(encrypted.Tag); break;
            case "key": encrypted.EncryptedKey = FlipByte(encrypted.EncryptedKey); break;
            case "name": info.Name = "another"; break;
            case "store": info.Store = "AzureKeyVault"; break;
            case "type": info.Type = nameof(RsaKeySecret); break;
            case "description": info.Description = "altered"; break;
            case "expiration": info.ExpiresUtc = DateTime.UtcNow; break;
        }

        await Assert.ThrowsAnyAsync<CryptographicException>(() => _service.DecryptAsync(encrypted, "key", info));
    }

    [Fact]
    public async Task DecryptAsync_RejectsLegacyUnauthenticatedEnvelope()
    {
        await Assert.ThrowsAsync<CryptographicException>(() => _service.DecryptAsync(new EncryptedSecretData(), "key",
            new SecretInfo { Name = "secret", Type = nameof(TextSecret) }));
    }

    private static string FlipByte(string value)
    {
        var bytes = Convert.FromBase64String(value);
        bytes[0] ^= 1;
        return Convert.ToBase64String(bytes);
    }

    public sealed class CustomSecret : ISecret
    {
        public string Value { get; set; }
    }

    private sealed class CustomSecretProvider : SecretTypeProvider<CustomSecret>
    {
        public override string DisplayName => "Custom";
        public override string Description => "Custom secret for regression coverage.";
    }
}
