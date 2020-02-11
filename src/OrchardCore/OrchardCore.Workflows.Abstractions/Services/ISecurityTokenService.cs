using System;

namespace OrchardCore.Workflows.Services
{
    public interface ISecurityTokenService
    {
        /// <summary>
        /// Creates a SAS (Shared Access Signature) token containing the specified data.
        /// </summary>
        string CreateToken<T>(T payload, TimeSpan lifetime);

        /// <summary>
        /// Decrypts the specified SAS token.
        /// </summary>
        bool TryDecryptToken<T>(string token, out T payload);
    }
}
