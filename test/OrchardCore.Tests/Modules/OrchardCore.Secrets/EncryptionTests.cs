using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Moq;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Services;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets
{
    public class EncryptionTests
    {
        private static ISecretService<RsaKeyPairSecret> GetSecretServiceMock()
        {
            using var rsa = RSA.Create();

            var rsaKeyPair = new RsaKeyPairSecret()
            {
                PublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
                PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey())
            };

            var secretService = Mock.Of<ISecretService<RsaKeyPairSecret>>();
            Mock.Get(secretService).Setup(s => s.GetSecretAsync("rsa")).ReturnsAsync(rsaKeyPair);

            return secretService;
        }

        [Fact]
        public async Task ShouldCreateEncryptor()
        {
            var secretService = GetSecretServiceMock();

            var encryptionProvider = new DefaultEncryptionProvider(secretService);

            using var encryptor = await encryptionProvider.CreateAsync("rsa");
            Assert.NotNull(encryptor);
            Assert.True(!String.IsNullOrEmpty(encryptor.EncryptionKey));
        }

        [Fact]
        public async Task ShouldEncrypt()
        {
            var secretService = GetSecretServiceMock();
            var encryptionProvider = new DefaultEncryptionProvider(secretService);

            using var encryptor = await encryptionProvider.CreateAsync("rsa");
            var encrypted = encryptor.Encrypt("foo");
            Assert.True(!String.IsNullOrEmpty(encrypted));
        }

        [Fact]
        public async Task ShouldEncryptThenDecrypt()
        {
            var secretService = GetSecretServiceMock();

            var encryptionProvider = new DefaultEncryptionProvider(secretService);

            using var encryptor = await encryptionProvider.CreateAsync("rsa");

            var encrypted = encryptor.Encrypt("foo");
            var decryptionProvider = new DefaultDecryptionProvider(secretService);
            using var decryptor = await decryptionProvider.CreateAsync(encryptor.EncryptionKey);
            var decrypted = decryptor.Decrypt(encrypted);
            Assert.Equal("foo", decrypted);
        }
    }
}
