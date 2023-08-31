using System;
using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets;

public interface ISecretStore
{
    string Name { get; }
    string DisplayName { get; }
    bool IsReadOnly { get; }
    Task<SecretBase> GetSecretAsync(string key, Type type);
    Task UpdateSecretAsync(string key, SecretBase secret);
    Task RemoveSecretAsync(string key);
}
