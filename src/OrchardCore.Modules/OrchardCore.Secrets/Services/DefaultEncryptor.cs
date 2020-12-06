using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace OrchardCore.Secrets.Services
{
    public class DefaultEncryptor : IEncryptor, IDisposable
    {
        private readonly ICryptoTransform _encryptor;
        private readonly string _secretName;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        private bool _disposedValue;

        public DefaultEncryptor(ICryptoTransform encryptor, string secretName, byte[] key, byte[] iv)
        {
            _encryptor = encryptor;
            _secretName = secretName;
            _key = key;
            _iv = iv;
        }

        public string Encrypt(string plainText)
        {
            byte[] encrypted;
            // byte[] hmacHash;

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, _encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }

            // using var hmac = new HMACSHA256();
            // using (var encryptedStream = new MemoryStream())
            // {
            //     using (var binaryWriter = new BinaryWriter(encryptedStream))
            //     {
            //         //Prepend IV
            //         binaryWriter.Write(_iv);
            //         //Write Ciphertext
            //         binaryWriter.Write(encrypted);
            //         binaryWriter.Flush();

            //         //Authenticate all data
            //         var tag = hmac.ComputeHash(encryptedStream.ToArray());
            //         //Postpend tag
            //         binaryWriter.Write(tag);
            //     }
            //     hmacHash = encryptedStream.ToArray();
            // }


            var descriptor = new HybridKeyDescriptor
            {
                SecretName = _secretName,
                Key = Convert.ToBase64String(_key),
                Iv = Convert.ToBase64String(_iv),
                ProtectedData = Convert.ToBase64String(encrypted)
            };

            var serialized = JsonConvert.SerializeObject(descriptor);
            var encodedDescriptor = Convert.ToBase64String(Encoding.UTF8.GetBytes(serialized));

            return encodedDescriptor;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _encryptor?.Dispose();
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
