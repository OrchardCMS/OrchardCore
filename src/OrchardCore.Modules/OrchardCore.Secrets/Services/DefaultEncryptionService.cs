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
        private readonly ISecretService<RsaSecret> _rsaSecretService;
        private readonly Dictionary<string, EncryptionKeyDescriptor> _encryptors = new Dictionary<string, EncryptionKeyDescriptor>();
        private bool _disposedValue;

        public DefaultEncryptionService(ISecretService<RsaSecret> rsaSecretService)
        {
            _rsaSecretService = rsaSecretService;
        }

        public async Task<string> EncryptAsync(string secretName, string protectedData)
        {
            if (_encryptors.TryGetValue(secretName, out var cachedDescriptor))
            {
                return Encrypt(cachedDescriptor, protectedData);
            }

            var secret = await _rsaSecretService.GetSecretAsync(secretName);
            if (secret == null)
            {
                throw new Exception("Secret not found " + secretName);
            }

            var keyDescriptor = new EncryptionKeyDescriptor();
            keyDescriptor.Rsa = RSA.Create();
            keyDescriptor.Rsa.KeySize = 2048;

            keyDescriptor.Rsa.ImportSubjectPublicKeyInfo(secret.PublicKeyAsBytes(), out _);
            // TODO lot of work to be done here. HMAC / Iv / Algorthim etc.
            keyDescriptor.Aes = Aes.Create();

            // Create an encryptor to perform the stream transform.
            keyDescriptor.Encryptor = keyDescriptor.Aes.CreateEncryptor(keyDescriptor.Aes .Key, keyDescriptor.Aes.IV);
            _encryptors.Add(secretName, keyDescriptor);

            return Encrypt(keyDescriptor, protectedData);

        }
        public string GetKey(string secretName)
        {
            if (_encryptors.TryGetValue(secretName, out var cachedDescriptor))
            {
                var key = cachedDescriptor.Rsa.Encrypt(cachedDescriptor.Aes.Key, RSAEncryptionPadding.Pkcs1);
                var iv = cachedDescriptor.Rsa.Encrypt(cachedDescriptor.Aes.IV, RSAEncryptionPadding.Pkcs1);
                var descriptor = new HybridKeyDescriptor
                {
                    SecretName = secretName,
                    Key = WebEncoders.Base64UrlEncode(key),
                    Iv = WebEncoders.Base64UrlEncode(iv)
                };
                var serialized = JsonConvert.SerializeObject(descriptor);

                return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(serialized));
            }

            throw new InvalidOperationException("Key not found. Encrypt some data before retriving key");

        }

        private string Encrypt(EncryptionKeyDescriptor keyWrapper, string plainText)
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

            return WebEncoders.Base64UrlEncode(encrypted);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach(var encryptor in _encryptors)
                    {
                        encryptor.Value.Dispose();
                    }
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
