using System;
using System.IO;
using System.Security.Cryptography;

namespace OrchardCore.Secrets.Services
{
    public class DefaultEncryptor : IEncryptor, IDisposable
    {
        public readonly ICryptoTransform _encryptor;
        public readonly string _keyDescriptor;
        private bool _disposedValue;

        public DefaultEncryptor(ICryptoTransform encryptor, string keyDescriptor)
        {
            _encryptor = encryptor;
            _keyDescriptor = keyDescriptor;
        }

        public string EncryptionKey => _keyDescriptor;

        public string Encrypt(string protectedData)
        {
            byte[] encrypted;

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, _encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(protectedData);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            
            return Convert.ToBase64String(encrypted);
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
