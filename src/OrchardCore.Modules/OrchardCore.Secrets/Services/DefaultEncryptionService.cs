using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OrchardCore.Secrets.Services
{
    public class DefaultEncryptionService : IEncryptionService, IDisposable
    {
        private readonly ISecretService<RsaKeyPair> _rsaSecretService;
        private EncryptionKeyDescriptor _encryptionKeyDescriptor;
        private bool _disposedValue;

        public DefaultEncryptionService(ISecretService<RsaKeyPair> rsaSecretService)
        {
            _rsaSecretService = rsaSecretService;
        }

        public async Task<string> InitializeAsync(string secretName)
        {
            var secret = await _rsaSecretService.GetSecretAsync(secretName);
            if (secret == null)
            {
                throw new Exception("Secret not found " + secretName);
            }

            // Don't assign descriptor until initialization complete.
            var keyDescriptor = new EncryptionKeyDescriptor();
            keyDescriptor.Rsa = RSA.Create();
            keyDescriptor.Rsa.KeySize = 2048;

            keyDescriptor.Rsa.ImportSubjectPublicKeyInfo(secret.PublicKeyAsBytes(), out _);
            // TODO lot of work to be done here. HMAC / Iv / Algorthim etc.
            keyDescriptor.Aes = Aes.Create();

            // Create an encryptor to perform the stream transform.
            keyDescriptor.Encryptor = keyDescriptor.Aes.CreateEncryptor(keyDescriptor.Aes.Key, keyDescriptor.Aes.IV);

            var key = keyDescriptor.Rsa.Encrypt(keyDescriptor.Aes.Key, RSAEncryptionPadding.Pkcs1);
            var iv = keyDescriptor.Rsa.Encrypt(keyDescriptor.Aes.IV, RSAEncryptionPadding.Pkcs1);
            var descriptor = new HybridKeyDescriptor
            {
                SecretName = secretName,
                Key = Convert.ToBase64String(key),
                Iv = Convert.ToBase64String(iv)
            };
            var serialized = JsonConvert.SerializeObject(descriptor);
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(serialized));

            _encryptionKeyDescriptor = keyDescriptor;

            return encoded;
        }

        public Task<string> EncryptAsync(string protectedData)
        {
            if (_encryptionKeyDescriptor == null)
            {
                throw new InvalidOperationException("Encryptor not initialized.");
            }

            return EncryptInternalAsync(_encryptionKeyDescriptor, protectedData);
        }

        private async Task<string> EncryptInternalAsync(EncryptionKeyDescriptor keyWrapper, string plainText)
        {
            byte[] encrypted;
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, keyWrapper.Encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);
            await swEncrypt.WriteAsync(plainText);
            encrypted = msEncrypt.ToArray();
                
            return Convert.ToBase64String(encrypted);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _encryptionKeyDescriptor?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    internal class EncryptionKeyDescriptor : IDisposable
    {
        private bool _disposedValue;

        public RSA Rsa { get; set; }
        public Aes Aes { get; set; }
        public ICryptoTransform Encryptor { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Rsa?.Dispose();
                    Aes?.Dispose();
                    Encryptor?.Dispose();

                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
