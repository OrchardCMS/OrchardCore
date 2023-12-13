using System;
using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets;

public interface ISecretStore
{
    string Name { get; }
    string DisplayName { get; }
    bool IsReadOnly { get; }
    Task<SecretBase> GetSecretAsync(string name, Type type);
    Task UpdateSecretAsync(string name, SecretBase secret);
    Task RemoveSecretAsync(string name);
}
