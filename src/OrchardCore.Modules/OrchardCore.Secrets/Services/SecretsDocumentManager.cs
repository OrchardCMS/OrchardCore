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
    public class SecretsDocumentManager
    {
        private const string CacheKey = nameof(SecretsDocumentManager);

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMemoryCache _memoryCache;

        public SecretsDocumentManager(
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
        public Task<SecretsDocument> LoadSecretsDocumentAsync() => _sessionHelper.LoadForUpdateAsync<SecretsDocument>();

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        public async Task<SecretsDocument> GetSecretsDocumentAsync()
        {
            if (!_memoryCache.TryGetValue<SecretsDocument>(CacheKey, out var document))
            {
                var changeToken = ChangeToken;

                document = await _sessionHelper.GetForCachingAsync<SecretsDocument>();

                foreach (var secret in document.Secrets.Values)
                {
                    secret.IsReadonly = true;
                }

                _memoryCache.Set(CacheKey, document, changeToken);
            }

            return document;
        }

        public async Task RemoveSecretAsync(string name)
        {
            var document = await LoadSecretsDocumentAsync();
            document.Secrets.Remove(name);

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }

        public async Task UpdateSecretAsync(string name, DocumentSecret secret)
        {
            if (secret.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var document = await LoadSecretsDocumentAsync();
            document.Secrets[name] = secret;

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }
    }
}
