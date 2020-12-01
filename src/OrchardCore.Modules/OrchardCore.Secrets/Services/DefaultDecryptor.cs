using System;
using System.IO;
using System.Security.Cryptography;

namespace OrchardCore.Secrets.Services
{
    public class DefaultDecryptor : IDecryptor, IDisposable
    {
        public readonly ICryptoTransform _decryptor;
        private bool _disposedValue;

        public DefaultDecryptor(ICryptoTransform decryptor)
        {
            _decryptor = decryptor;
        }

        public string Decrypt(string protectedData)
        {
            var protectedBytes = Convert.FromBase64String(protectedData);
            string plaintext = null;
            using var msDecrypt = new MemoryStream(protectedBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, _decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            plaintext = srDecrypt.ReadToEnd();

            return plaintext;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _decryptor?.Dispose();
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
