using System.Security.Cryptography;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets
{
    public class EncryptionTests
    {
        private static ISecretService GetSecretServiceMock()
        {
            using var rsaEncryptor = RSAGenerator.GenerateRSASecurityKey(2048);
            var encryptionSecret = new RSASecret()
            {
                Name = "Tests.Encryption",
                PublicKey = Convert.ToBase64String(rsaEncryptor.ExportRSAPublicKey()),
                PrivateKey = Convert.ToBase64String(rsaEncryptor.ExportRSAPrivateKey()),
                KeyType = RSAKeyType.PublicPrivate,
            };

            using var rsaSigning = RSAGenerator.GenerateRSASecurityKey(2048);
            var signingSecret = new RSASecret()
            {
                Name = "Tests.Signing",
                PublicKey = Convert.ToBase64String(rsaSigning.ExportRSAPublicKey()),
                PrivateKey = Convert.ToBase64String(rsaSigning.ExportRSAPrivateKey()),
                KeyType = RSAKeyType.PublicPrivate,
            };

            var encryptionInfo = new SecretInfo() { Name = "Tests.Encryption" };
            var signingInfo = new SecretInfo() { Name = "Tests.Signing" };
            var secrets = new Dictionary<string, SecretInfo>()
            {
                { "Tests.Encryption", encryptionInfo },
                { "Tests.Signing", signingInfo },
            };

            var secretService = Mock.Of<ISecretService>();

            Mock.Get(secretService).Setup(s => s.GetSecretInfosAsync()).ReturnsAsync(secrets);
            Mock.Get(secretService).Setup(s => s.GetSecretAsync(encryptionSecret.Name)).ReturnsAsync(encryptionSecret);
            Mock.Get(secretService).Setup(s => s.GetSecretAsync(signingSecret.Name)).ReturnsAsync(signingSecret);

            return secretService;
        }

        [Fact]
        public async Task ShouldEncrypt()
        {
            var protectionProvider = new SecretProtectionProvider(GetSecretServiceMock());
            var protector = protectionProvider.CreateProtector("Tests");
            var encrypted = await protector.ProtectAsync("foo");

            Assert.False(string.IsNullOrEmpty(encrypted));
        }

        [Fact]
        public async Task ShouldEncryptThenDecrypt()
        {
            var protectionProvider = new SecretProtectionProvider(GetSecretServiceMock());
            var protector = protectionProvider.CreateProtector("Tests");

            var encrypted = await protector.ProtectAsync("foo");
            var (Plaintext, _) = await protector.UnprotectAsync(encrypted);

            Assert.Equal("foo", Plaintext);
        }

        [Fact]
        public async Task ShouldThrowWhenDecryptingWithBaDKeys()
        {
            var protectionProvider = new SecretProtectionProvider(GetSecretServiceMock());
            var protector = protectionProvider.CreateProtector("Tests");

            var encrypted = await protector.ProtectAsync("foo");

            // Generate new keys for decryption, which will cause the unprotector to throw.
            protectionProvider = new SecretProtectionProvider(GetSecretServiceMock());
            protector = protectionProvider.CreateProtector("Tests");

            await Assert.ThrowsAsync<CryptographicException>(() => protector.UnprotectAsync(encrypted));
        }
    }
}
