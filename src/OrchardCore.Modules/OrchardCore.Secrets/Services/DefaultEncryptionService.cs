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
                Key = WebEncoders.Base64UrlEncode(key),
                Iv = WebEncoders.Base64UrlEncode(iv)
            };
            var serialized = JsonConvert.SerializeObject(descriptor);
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(serialized));

            _encryptionKeyDescriptor = keyDescriptor;

            return encoded;
        }

        public Task<string> EncryptAsync(string protectedData)
        {
            if (_encryptionKeyDescriptor == null)
            {
                throw new InvalidOperationException("Encryptor not initialized.");
            }

            return Task.FromResult(EncryptInternal(_encryptionKeyDescriptor, protectedData));
        }

        // TODO can be async.
        private string EncryptInternal(EncryptionKeyDescriptor keyWrapper, string plainText)
        {
            byte[] encrypted;
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, keyWrapper.Encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }

            // TODO we shouldn't use weburlencoding, just base64.
            return WebEncoders.Base64UrlEncode(encrypted);

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
