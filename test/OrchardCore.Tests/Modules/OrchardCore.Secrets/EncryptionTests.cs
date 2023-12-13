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
                Name = "rsaencryptor",
                PublicKey = Convert.ToBase64String(rsaEncryptor.ExportRSAPublicKey()),
                PrivateKey = Convert.ToBase64String(rsaEncryptor.ExportRSAPrivateKey()),
                KeyType = RSAKeyType.PublicPrivate,
            };

            using var rsaSigning = RSAGenerator.GenerateRSASecurityKey(2048);
            var signingSecret = new RSASecret()
            {
                Name = "rsasigning",
                PublicKey = Convert.ToBase64String(rsaSigning.ExportRSAPublicKey()),
                PrivateKey = Convert.ToBase64String(rsaSigning.ExportRSAPrivateKey()),
                KeyType = RSAKeyType.PublicPrivate,
            };

            var encryptionInfo = new SecretInfo() { Name = "rsaencryptor" };
            var signingInfo = new SecretInfo() { Name = "rsasigning" };
            var secrets = new Dictionary<string, SecretInfo>()
            {
                { "rsaencryptor", encryptionInfo },
                { "rsasigning", signingInfo },
            };

            var secretService = Mock.Of<ISecretService>();

            Mock.Get(secretService).Setup(s => s.GetSecretInfosAsync()).ReturnsAsync(secrets);
            Mock.Get(secretService).Setup(s => s.GetSecretAsync<RSASecret>(encryptionSecret.Name)).ReturnsAsync(encryptionSecret);
            Mock.Get(secretService).Setup(s => s.GetSecretAsync<RSASecret>(signingSecret.Name)).ReturnsAsync(signingSecret);

            return secretService;
        }

        [Fact]
        public async Task ShouldEncrypt()
        {
            var protectionProvider = new SecretProtectionProvider(GetSecretServiceMock());
            var encryptor = await protectionProvider.CreateEncryptorAsync("rsaencryptor", "rsasigning");
            var encrypted = encryptor.Encrypt("foo");

            Assert.True(!String.IsNullOrEmpty(encrypted));
        }

        [Fact]
        public async Task ShouldEncryptThenDecrypt()
        {
            var protectionProvider = new SecretProtectionProvider(GetSecretServiceMock());
            var encryptor = await protectionProvider.CreateEncryptorAsync("rsaencryptor", "rsasigning");
            var encrypted = encryptor.Encrypt("foo");

            var decryptor = await protectionProvider.CreateDecryptorAsync(encrypted);
            var decrypted = decryptor.Decrypt();

            Assert.Equal("foo", decrypted);
        }

        [Fact]
        public async Task ShouldThrowWhenDecryptingWithBaDKeys()
        {
            var protectionProvider = new SecretProtectionProvider(GetSecretServiceMock());
            var encryptor = await protectionProvider.CreateEncryptorAsync("rsaencryptor", "rsasigning");
            var encrypted = encryptor.Encrypt("foo");

            // Generate new keys for decryption, which will cause the decryptor to throw.
            protectionProvider = new SecretProtectionProvider(GetSecretServiceMock());
            var decryptor = await protectionProvider.CreateDecryptorAsync(encrypted);

            Assert.Throws<CryptographicException>(() => decryptor.Decrypt());
        }
    }
}
