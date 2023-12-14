using System;
using System.Threading.Tasks;

namespace OrchardCore.Workflows.Services
{
    public interface ISecretTokenService
    {
        /// <summary>
        /// Creates a SAS (Shared Access Signature) token containing the specified data.
        /// </summary>
        Task<string> CreateTokenAsync<T>(T payload, TimeSpan lifetime);

        /// <summary>
        /// Decrypts the specified SAS token.
        /// </summary>
        Task<(bool, T)> TryDecryptTokenAsync<T>(string token);
    }
}
