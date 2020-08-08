using System;
using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public interface ISecretStore
    {
        string Name { get; }
        string DisplayName { get; }
        bool IsReadOnly { get; }
        Task<Secret> GetSecretAsync(string key, Type type);
        Task<TSecret> GetSecretAsync<TSecret>(string key) where TSecret : Secret, new();
        Task UpdateSecretAsync(string key, Secret secret);
        Task RemoveSecretAsync(string key);
    }

    public class SecretStoreDescriptor
    {
        public string Name { get; set; }
        public string DisplayName { get; set;}
        public bool IsReadOnly { get; set; }
    }
}
