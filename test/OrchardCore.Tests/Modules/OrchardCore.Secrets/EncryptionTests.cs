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
        private static ISecretService<RsaSecret> GetSecretServiceMock()
        {
            using var rsa = RSA.Create();

            var rsaSecret = new RsaSecret()
            {
                PublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
                PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey())
            };

            var secretService = Mock.Of<ISecretService<RsaSecret>>();
            Mock.Get(secretService).Setup(s => s.GetSecretAsync("rsa")).ReturnsAsync(rsaSecret);

            return secretService;
        }

        [Fact]
        public async Task ShouldCreateEncryptor()
        {
            var secretService = GetSecretServiceMock();

            var encryptionProvider = new DefaultEncryptionProvider(secretService);

            using var encryptor = await encryptionProvider.CreateAsync("rsa");
            Assert.NotNull(encryptor);
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
            using var decryptor = await decryptionProvider.CreateAsync(encrypted);
            var decrypted = decryptor.Decrypt(encrypted);
            Assert.Equal("foo", decrypted);
        }
    }
}
