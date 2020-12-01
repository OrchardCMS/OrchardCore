using System;

namespace OrchardCore.Secrets
{
    public interface IDecryptor : IDisposable
    {
        string Decrypt(string protectedData);
    }
}
