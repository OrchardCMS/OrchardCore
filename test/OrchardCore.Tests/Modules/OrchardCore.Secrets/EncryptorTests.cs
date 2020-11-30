using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
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
                var rsaKeyPair = new RsaKeyPair()
                {
                    PublicKey = WebEncoders.Base64UrlEncode(rsa.ExportSubjectPublicKeyInfo()),
                    PrivateKey = WebEncoders.Base64UrlEncode(rsa.ExportRSAPrivateKey())
                };

                var secretService = Mock.Of<ISecretService<RsaKeyPair>>();

                Mock.Get(secretService).Setup(s => s.GetSecretAsync("rsa")).ReturnsAsync(rsaKeyPair);

                using (var encryptionService = new DefaultEncryptionService(secretService))
                {
                    var aesKey = await encryptionService.InitializeAsync("rsa");
                    var encrypted = await encryptionService.EncryptAsync("foo");
                    using (var decryptionService = new DefaultDecryptionService(secretService))
                    {
                        var decrypted = await decryptionService.DecryptAsync(aesKey, encrypted);
                        Assert.Equal("foo", decrypted);
                    }
                }
            }
        }
    }
}
