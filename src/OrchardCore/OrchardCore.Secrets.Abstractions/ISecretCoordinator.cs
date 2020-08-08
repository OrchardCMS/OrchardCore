using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public interface ISecretCoordinator : IEnumerable<SecretStoreDescriptor>
    {
        Task<IDictionary<string, SecretBinding>> GetSecretBindingsAsync();
        Task<IDictionary<string, SecretBinding>> LoadSecretBindingsAsync();
        Task UpdateSecretAsync(string key, SecretBinding secretBinding, Secret secret);
        Task RemoveSecretAsync(string key, string store);
        Task<Secret> GetSecretAsync(string key, Type type);
        Task<TSecret> GetSecretAsync<TSecret>(string key) where TSecret : Secret, new();
    }
}
