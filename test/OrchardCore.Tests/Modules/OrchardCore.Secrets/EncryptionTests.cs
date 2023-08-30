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
            using var rsaEncryptor = RsaHelper.GenerateRsaSecurityKey(2048);

            var rsaEncryptionSecret = new RsaSecret()
            {
                PublicKey = Convert.ToBase64String(rsaEncryptor.ExportRSAPublicKey()),
                PrivateKey = Convert.ToBase64String(rsaEncryptor.ExportRSAPrivateKey()),
            };

            using var rsaSigning = RsaHelper.GenerateRsaSecurityKey(2048);
            var rsaSigningSecret = new RsaSecret()
            {
                PublicKey = Convert.ToBase64String(rsaSigning.ExportRSAPublicKey()),
                PrivateKey = Convert.ToBase64String(rsaSigning.ExportRSAPrivateKey()),
            };

            var secretService = Mock.Of<ISecretService>();
            Mock.Get(secretService).Setup(s => s.GetSecretAsync("rsaencryptor", typeof(RsaSecret))).ReturnsAsync(rsaEncryptionSecret);
            Mock.Get(secretService).Setup(s => s.GetSecretAsync("rsasigning", typeof(RsaSecret))).ReturnsAsync(rsaSigningSecret);

            return secretService;
        }

        [Fact]
        public async Task ShouldCreateEncryptor()
        {
            var secretService = GetSecretServiceMock();

            var encryptionProvider = new DefaultEncryptionProvider(secretService);

            var encryptor = await encryptionProvider.CreateAsync("rsaencryptor", "rsasigning");

            Assert.NotNull(encryptor);
        }

        [Fact]
        public async Task ShouldEncrypt()
        {
            var secretService = GetSecretServiceMock();
            var encryptionProvider = new DefaultEncryptionProvider(secretService);

            var encryptor = await encryptionProvider.CreateAsync("rsaencryptor", "rsasigning");
            var encrypted = encryptor.Encrypt("foo");

            Assert.True(!String.IsNullOrEmpty(encrypted));
        }

        [Fact]
        public async Task ShouldEncryptThenDecrypt()
        {
            var secretService = GetSecretServiceMock();

            var encryptionProvider = new DefaultEncryptionProvider(secretService);

            var encryptor = await encryptionProvider.CreateAsync("rsaencryptor", "rsasigning");

            var encrypted = encryptor.Encrypt("foo");
            var decryptionProvider = new DefaultDecryptionProvider(secretService);
            var decryptor = await decryptionProvider.CreateAsync(encrypted);
            var decrypted = decryptor.Decrypt(encrypted);

            Assert.Equal("foo", decrypted);
        }

        [Fact]
        public async Task ShouldThrowWhenDecryptingWithBaDKeys()
        {
            var encryptionSecretService = GetSecretServiceMock();

            var encryptionProvider = new DefaultEncryptionProvider(encryptionSecretService);

            var encryptor = await encryptionProvider.CreateAsync("rsaencryptor", "rsasigning");

            var encrypted = encryptor.Encrypt("foo");

            // Generate new keys for decryption, which will cause the decryptor to throw.
            var decryptionSecretService = GetSecretServiceMock();

            var decryptionProvider = new DefaultDecryptionProvider(decryptionSecretService);
            var decryptor = await decryptionProvider.CreateAsync(encrypted);

            Assert.Throws<CryptographicException>(() => decryptor.Decrypt(encrypted));
        }
    }
}
