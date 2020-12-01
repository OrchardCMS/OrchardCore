using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Moq;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Services;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets
{
    public class EncryptorTests
    {
        [Fact]
        public async Task ShouldEncryptThenDecrypt()
        {
            // This needs to be a better key.
            using (var rsa = RSA.Create())
            {
                var rsaKeyPair = new RsaKeyPairSecret()
                {
                    PublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
                    PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey())
                };

                var secretService = Mock.Of<ISecretService<RsaKeyPairSecret>>();

                Mock.Get(secretService).Setup(s => s.GetSecretAsync("rsa")).ReturnsAsync(rsaKeyPair);

                using (var encryptionService = new DefaultEncryptionService(secretService))
                {
                    var aesKey = await encryptionService.InitializeAsync("rsa");
                    Assert.False(String.IsNullOrEmpty(aesKey));
                    var encrypted = await encryptionService.EncryptAsync("foo");
                    Assert.False(String.IsNullOrEmpty(encrypted));
                    using (var decryptionService = new DefaultDecryptionService(secretService))
                    {
                        var decrypted = await decryptionService.DecryptAsync(aesKey, encrypted);
                        Assert.False(String.IsNullOrEmpty(decrypted));
                        Assert.Equal("foo", decrypted);
                    }
                }
            }
        }
    }
}
