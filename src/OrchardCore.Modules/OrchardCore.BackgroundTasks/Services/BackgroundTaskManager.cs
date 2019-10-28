using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.BackgroundTasks.Models;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using YesSql;

namespace OrchardCore.BackgroundTasks.Services
{
    public class BackgroundTaskManager
    {
        private const string CacheKey = nameof(BackgroundTaskManager);

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMemoryCache _memoryCache;

        public BackgroundTaskManager(
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
        public Task<BackgroundTaskDocument> LoadDocumentAsync() => _sessionHelper.LoadForUpdateAsync<BackgroundTaskDocument>();

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        /// <inheritdoc/>
        public async Task<BackgroundTaskDocument> GetDocumentAsync()
        {
            if (!_memoryCache.TryGetValue<BackgroundTaskDocument>(CacheKey, out var document))
            {
                var changeToken = ChangeToken;

                document = await _sessionHelper.GetForCachingAsync<BackgroundTaskDocument>();

                foreach (var settings in document.Settings.Values)
                {
                    settings.IsReadonly = true;
                }

                _memoryCache.Set(CacheKey, document, changeToken);
            }

            return document;
        }

        public async Task RemoveAsync(string name)
        {
            var document = await LoadDocumentAsync();
            document.Settings.Remove(name);

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }

        public async Task UpdateAsync(string name, BackgroundTaskSettings settings)
        {
            if (settings.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var document = await LoadDocumentAsync();
            document.Settings[name] = settings;

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }
    }
}
