using System;

namespace OrchardCore.Secrets
{
    public interface IEncryptor : IDisposable
    {
        string Encrypt(string plainText);
        string EncryptionKey { get; }
    }
}
