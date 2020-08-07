using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public interface ISecretCoordinator : IEnumerable<SecretStoreDescriptor>
    {
        Task UpdateSecretAsync(string key, string store, Secret secret);
        Task RemoveSecretAsync(string key, string store);
        Task<TSecret> GetSecretAsync<TSecret>(string key) where TSecret : Secret, new();
    }
}
