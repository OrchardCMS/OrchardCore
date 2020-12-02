using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OrchardCore.Secrets.Services
{
    public class DefaultDecryptionProvider : IDecryptionProvider
    {
        private readonly ISecretService<RsaSecret> _rsaSecretService;

        public DefaultDecryptionProvider(ISecretService<RsaSecret> rsaSecretService)
        {
            _rsaSecretService = rsaSecretService;
        }

        public async Task<IDecryptor> CreateAsync(string protectedData)
        {
            var bytes = Convert.FromBase64String(protectedData);
            var decoded = Encoding.UTF8.GetString(bytes);

            var descriptor = JsonConvert.DeserializeObject<HybridKeyDescriptor>(decoded);

            var secret = await _rsaSecretService.GetSecretAsync(descriptor.SecretName);
            if (secret == null)
            {
                throw new InvalidOperationException($"{descriptor.SecretName} secret not found");
            }

            using var rsa = RSA.Create();

            rsa.KeySize = 2048;

            rsa.ImportRSAPrivateKey(Convert.FromBase64String(secret.PrivateKey), out _);

            var key = rsa.Decrypt(Convert.FromBase64String(descriptor.Key), RSAEncryptionPadding.Pkcs1);
            var iv = rsa.Decrypt(Convert.FromBase64String(descriptor.Iv), RSAEncryptionPadding.Pkcs1);

            using var aes = Aes.Create();

            return new DefaultDecryptor(aes.CreateDecryptor(key, iv));
        }
    }
}
