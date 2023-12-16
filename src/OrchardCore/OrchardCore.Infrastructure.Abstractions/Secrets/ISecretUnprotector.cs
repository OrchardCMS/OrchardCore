using System;
using System.Threading.Tasks;

namespace OrchardCore.Secrets;

public interface ISecretUnprotector
{
    Task<(string Plaintext, DateTimeOffset Expiration)> UnprotectAsync();
}
