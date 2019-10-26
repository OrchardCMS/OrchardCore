using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.BackgroundTasks.Models;
using OrchardCore.Environment.Cache;
using YesSql;

namespace OrchardCore.BackgroundTasks.Services
{
    public class BackgroundTaskManager
    {
        private const string CacheKey = nameof(BackgroundTaskManager);

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly IMemoryCache _memoryCache;

        private BackgroundTaskDocument _backgroundTaskDocument;

        public BackgroundTaskManager(
            ISignal signal,
            ISession session,
            IMemoryCache memoryCache)
        {
            _signal = signal;
            _session = session;
            _memoryCache = memoryCache;
        }

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        /// <summary>
        /// Returns the document from the database to be updated.
        /// </summary>
        public async Task<BackgroundTaskDocument> LoadDocumentAsync()
        {
            return _backgroundTaskDocument = _backgroundTaskDocument ?? await _session.Query<BackgroundTaskDocument>().FirstOrDefaultAsync() ?? new BackgroundTaskDocument();
        }

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        /// <inheritdoc/>
        public async Task<BackgroundTaskDocument> GetDocumentAsync()
        {
            if (!_memoryCache.TryGetValue<BackgroundTaskDocument>(CacheKey, out var document))
            {
                var changeToken = ChangeToken;

                if (_backgroundTaskDocument != null)
                {
                    _session.Detach(_backgroundTaskDocument);
                }

                document = await _session.Query<BackgroundTaskDocument>().FirstOrDefaultAsync();

                if (document != null)
                {
                    _session.Detach(document);

                    foreach (var settings in document.Settings.Values)
                    {
                        settings.IsReadonly = true;
                    }
                }
                else
                {
                    document = new BackgroundTaskDocument();
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
