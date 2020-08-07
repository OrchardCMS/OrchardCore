using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Secrets.Services
{
    public class DefaultSecretCoordinator : ISecretCoordinator
    {
        private readonly IEnumerable<ISecretStore> _secretStores;

        public DefaultSecretCoordinator(
            IEnumerable<ISecretStore> secretStores)
        {
            _secretStores = secretStores;
        }

        public Task UpdateSecretAsync(string key, string store, Secret secret)
        {
            var secretStore = _secretStores.FirstOrDefault(x => String.Equals(x.Name, store, StringComparison.OrdinalIgnoreCase));
            if (secretStore != null && !secretStore.IsReadOnly)
            {
                return secretStore.UpdateSecretAsync(key, secret);
            }

            // This is a noop rather than an exception as updating a readonly store is considered a noop.
            return Task.CompletedTask;
        }

        public Task RemoveSecretAsync(string key, string store)
        {
            var secretStore = _secretStores.FirstOrDefault(x => String.Equals(x.Name, store, StringComparison.OrdinalIgnoreCase));
            if (secretStore != null && !secretStore.IsReadOnly)
            {
                return secretStore.RemoveSecretAsync(key);
            }

            // This is a noop rather than an exception as updating a readonly store is considered a noop.
            return Task.CompletedTask;
        }

        public async Task<TSecret> GetSecretAsync<TSecret>(string key) where TSecret : Secret, new()
        {
            foreach(var secretStore in _secretStores)
            {
                var secret = await secretStore.GetSecretAsync<TSecret>(key);
                if (secret != null)
                {
                    return secret;
                }
            }

            return null;
        }
        public IEnumerator<SecretStoreDescriptor> GetEnumerator()
        {
            return _secretStores.Select(x => new SecretStoreDescriptor { Name = x.Name, IsReadOnly = x.IsReadOnly, DisplayName = x.DisplayName }).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
