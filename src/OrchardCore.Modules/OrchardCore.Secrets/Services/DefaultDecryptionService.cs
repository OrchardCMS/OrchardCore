using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace OrchardCore.Secrets.Services
{
    public class DefaultDecryptionService : IDecryptionService, IDisposable
    {
        private readonly ISecretService<RsaSecret> _rsaSecretService;
        private readonly Dictionary<string, ICryptoTransform> _decryptors = new Dictionary<string, ICryptoTransform>();
        private bool disposedValue;

        public DefaultDecryptionService(ISecretService<RsaSecret> rsaSecretService)
        {
            _rsaSecretService = rsaSecretService;
        }

        public async Task<string> DecryptAsync(string encryptionKey, string protectedData)
        {
            if (_decryptors.TryGetValue(encryptionKey, out var cachedDecryptor))
            {
                return Decrypt(cachedDecryptor, protectedData);
            }

            var bytes = WebEncoders.Base64UrlDecode(encryptionKey);
            var decoded = Encoding.UTF8.GetString(bytes);

            var descriptor = JsonConvert.DeserializeObject<HybridKeyDescriptor>(decoded);

            using (var rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                var secret = await _rsaSecretService.GetSecretAsync(descriptor.SecretName);

                rsa.ImportRSAPrivateKey(WebEncoders.Base64UrlDecode(secret.PrivateKey), out _);

                var key = rsa.Decrypt(WebEncoders.Base64UrlDecode(descriptor.Key), RSAEncryptionPadding.Pkcs1);
                var iv = rsa.Decrypt(WebEncoders.Base64UrlDecode(descriptor.Iv), RSAEncryptionPadding.Pkcs1);

                using (var aes = Aes.Create())
                {
                    var decryptor = aes.CreateDecryptor(key, iv);
                    _decryptors.Add(encryptionKey, decryptor);

                    return Decrypt(decryptor, protectedData);
                }
            }
        }

        private string Decrypt(ICryptoTransform decryptor, string protectedData)
        {
            var protectedBytes = WebEncoders.Base64UrlDecode(protectedData);
            string plaintext = null;
            using (MemoryStream msDecrypt = new MemoryStream(protectedBytes))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }

            return plaintext;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(var decryptor in _decryptors)
                    {
                        decryptor.Value.Dispose();
                    }
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
