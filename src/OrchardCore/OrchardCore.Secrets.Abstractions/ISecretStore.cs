using System;
using System.Threading.Tasks;

namespace OrchardCore.Secrets;

public interface ISecretStore
{
    string Name { get; }
    string DisplayName { get; }
    bool IsReadOnly { get; }
    Task<Secret> GetSecretAsync(string key, Type type);
    Task UpdateSecretAsync(string key, Secret secret);
    Task RemoveSecretAsync(string key);
}
