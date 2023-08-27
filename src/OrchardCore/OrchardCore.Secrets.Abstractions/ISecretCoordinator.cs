using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Secrets;

public interface ISecretCoordinator
{
    public Secret CreateSecret(string typeName);
    Task<Secret> GetSecretAsync(string key, Type type);
    Task<Secret> GetSecretAsync(SecretBinding binding);
    public TSecret CreateSecret<TSecret>() where TSecret : Secret, new();
    Task<TSecret> GetSecretAsync<TSecret>(string key) where TSecret : Secret, new();
    Task<IDictionary<string, SecretBinding>> GetSecretBindingsAsync();
    Task<IDictionary<string, SecretBinding>> LoadSecretBindingsAsync();
    IReadOnlyCollection<SecretStoreDescriptor> GetSecretStoreDescriptors();
    Task UpdateSecretAsync(string key, SecretBinding secretBinding, Secret secret);
    Task RemoveSecretAsync(string key, string storeName);
}
