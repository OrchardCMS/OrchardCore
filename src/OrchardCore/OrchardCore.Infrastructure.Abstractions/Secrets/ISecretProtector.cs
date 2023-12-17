using System;
using System.Threading.Tasks;

namespace OrchardCore.Secrets;

public interface ISecretProtector
{
    Task<string> ProtectAsync(string plaintext, DateTimeOffset? expiration = null);
    Task<(string Plaintext, DateTimeOffset Expiration)> UnprotectAsync(string protectedData);
}
