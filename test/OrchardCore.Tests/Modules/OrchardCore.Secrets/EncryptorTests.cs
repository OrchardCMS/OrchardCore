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
                var rsaSecret = new RsaSecret()
                {
                    PublicKey = WebEncoders.Base64UrlEncode(rsa.ExportSubjectPublicKeyInfo()),
                    PrivateKey = WebEncoders.Base64UrlEncode(rsa.ExportRSAPrivateKey())
                };

                var secretService = Mock.Of<ISecretService<RsaSecret>>();

                Mock.Get(secretService).Setup(s => s.GetSecretAsync("rsa")).ReturnsAsync(rsaSecret);

                using (var encryptionService = new DefaultEncryptionService(secretService))
                {
                    var encrypted = await encryptionService.EncryptAsync("rsa", "foo");
                    var key = encryptionService.GetKey("rsa");
                    using (var decryptionService = new DefaultDecryptionService(secretService))
                    {
                        var decrypted = await decryptionService.DecryptAsync(key, encrypted);
                        Assert.Equal("foo", decrypted);
                    }
                }
            }
        }
    }
}
