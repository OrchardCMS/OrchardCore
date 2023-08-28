using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets;

public interface ISecretService
{
    public Secret CreateSecret(string typeName);
    Task<Secret> GetSecretAsync(string key, Type type);
    Task<Secret> GetSecretAsync(SecretBinding binding);
    Task<IDictionary<string, SecretBinding>> GetSecretBindingsAsync();
    Task<IDictionary<string, SecretBinding>> LoadSecretBindingsAsync();
    IReadOnlyCollection<SecretStoreDescriptor> GetSecretStoreDescriptors();
    Task UpdateSecretAsync(string key, SecretBinding secretBinding, Secret secret);
    Task RemoveSecretAsync(string key, string storeName);
}
