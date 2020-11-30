using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OrchardCore.Secrets.Services
{
    public class DefaultDecryptionService : IDecryptionService, IDisposable
    {
        private readonly ISecretService<RsaKeyPair> _rsaSecretService;
        // TODO will also have to check for RsaPublicKey - for when they are stored without the private.
        private ICryptoTransform _decryptor;
        private bool disposedValue;

        public DefaultDecryptionService(ISecretService<RsaKeyPair> rsaSecretService)
        {
            _rsaSecretService = rsaSecretService;
        }

        public async Task<string> DecryptAsync(string encryptionKey, string protectedData)
        {
            var bytes = Convert.FromBase64String(encryptionKey);
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
                
            _decryptor = aes.CreateDecryptor(key, iv);

            return await DecryptInternalAsync(_decryptor, protectedData);
        }

        private async Task<string> DecryptInternalAsync(ICryptoTransform decryptor, string protectedData)
        {
            var protectedBytes = Convert.FromBase64String(protectedData);
            string plaintext = null;
            using var msDecrypt = new MemoryStream(protectedBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            plaintext = await srDecrypt.ReadToEndAsync();

            return plaintext;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _decryptor?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
