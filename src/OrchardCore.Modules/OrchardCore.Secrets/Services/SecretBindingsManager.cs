using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using OrchardCore.Secrets.Models;
using YesSql;

namespace OrchardCore.Secrets.Services
{
    public class SecretBindingsManager
    {
        private const string CacheKey = nameof(SecretBindingsManager);

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMemoryCache _memoryCache;

        public SecretBindingsManager(
            ISignal signal,
            ISession session,
            ISessionHelper sessionHelper,
            IMemoryCache memoryCache)
        {
            _signal = signal;
            _session = session;
            _sessionHelper = sessionHelper;
            _memoryCache = memoryCache;
        }

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        /// <summary>
        /// Returns the document from the database to be updated.
        /// </summary>
        public Task<SecretBindingsDocument> LoadSecretBindingsDocumentAsync() => _sessionHelper.LoadForUpdateAsync<SecretBindingsDocument>();

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        public async Task<SecretBindingsDocument> GetSecretBindingsDocumentAsync()
        {
            if (!_memoryCache.TryGetValue<SecretBindingsDocument>(CacheKey, out var document))
            {
                var changeToken = ChangeToken;

                document = await _sessionHelper.GetForCachingAsync<SecretBindingsDocument>();

                foreach (var secretBinding in document.SecretBindings.Values)
                {
                    secretBinding.IsReadonly = true;
                }

                _memoryCache.Set(CacheKey, document, changeToken);
            }

            return document;
        }

        public async Task RemoveSecretBindingAsync(string name)
        {
            var document = await LoadSecretBindingsDocumentAsync();
            document.SecretBindings.Remove(name);

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }

        public async Task UpdateSecretBindingAsync(string name, SecretBinding secretBinding)
        {
            if (secretBinding.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var document = await LoadSecretBindingsDocumentAsync();
            document.SecretBindings[name] = secretBinding;

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }
    }
}
