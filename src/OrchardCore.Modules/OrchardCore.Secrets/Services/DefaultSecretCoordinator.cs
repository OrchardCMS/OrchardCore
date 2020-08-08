using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Secrets.Services
{
    public class DefaultSecretCoordinator : ISecretCoordinator
    {
        private readonly SecretBindingsManager _secretBindingsManager;
        private readonly IEnumerable<ISecretStore> _secretStores;

        public DefaultSecretCoordinator(
            SecretBindingsManager secretBindingsManager,
            IEnumerable<ISecretStore> secretStores)
        {
            _secretBindingsManager = secretBindingsManager;
            _secretStores = secretStores;
        }

        public async Task<IDictionary<string, SecretBinding>> GetSecretBindingsAsync()
        {
            var secretsDocument = await _secretBindingsManager.GetSecretBindingsDocumentAsync();
            return secretsDocument.SecretBindings;
        }

        public async Task<IDictionary<string, SecretBinding>> LoadSecretBindingsAsync()
        {
            var secretsDocument = await _secretBindingsManager.LoadSecretBindingsDocumentAsync();
            return secretsDocument.SecretBindings;
        }

        public async Task UpdateSecretAsync(string key, SecretBinding secretBinding, Secret secret)
        {
            var secretStore = _secretStores.FirstOrDefault(x => String.Equals(x.Name, secretBinding.Store, StringComparison.OrdinalIgnoreCase));
            if (secretStore != null)
            {
                await _secretBindingsManager.UpdateSecretBindingAsync(key, secretBinding);
                // This is a noop rather than an exception as updating a readonly store is considered a noop.
                if (!secretStore.IsReadOnly)
                {
                    await secretStore.UpdateSecretAsync(key, secret);
                }
            }
            else
            {
                throw new InvalidOperationException("The specified store was not found");
            }
        }

        public async Task RemoveSecretAsync(string key, string store)
        {
            var secretStore = _secretStores.FirstOrDefault(x => String.Equals(x.Name, store, StringComparison.OrdinalIgnoreCase));
            if (secretStore != null && !secretStore.IsReadOnly)
            {
                await _secretBindingsManager.RemoveSecretBindingAsync(key);
                if (!secretStore.IsReadOnly)
                {
                    // This is a noop rather than an exception as updating a readonly store is considered a noop.
                    await secretStore.RemoveSecretAsync(key);
                }
            }
            else
            {
                throw new InvalidOperationException("The specified store was not found");
            }
        }

        public async Task<Secret> GetSecretAsync(string key, Type type)
        {
            foreach(var secretStore in _secretStores)
            {
                var secret = await secretStore.GetSecretAsync(key, type);
                if (secret != null)
                {
                    return secret;
                }
            }

            return null;
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
