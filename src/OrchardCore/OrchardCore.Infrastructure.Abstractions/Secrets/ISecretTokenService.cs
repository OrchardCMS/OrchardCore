using System;
using System.Threading.Tasks;

namespace OrchardCore.Secrets.Services;

public interface ISecretTokenService
{
    /// <summary>
    /// Creates a secret token containing the specified data.
    /// </summary>
    Task<string> CreateTokenAsync<T>(T payload, TimeSpan lifetime);

    /// <summary>
    /// Decrypts the specified secret token.
    /// </summary>
    Task<(bool, T)> TryDecryptTokenAsync<T>(string token);
}
