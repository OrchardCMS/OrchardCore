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
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly ISession _session;

        private const string CacheKey = nameof(BackgroundTaskManager);

        public BackgroundTaskManager(
            IMemoryCache memoryCache,
            ISignal signal,
            ISession session)
        {
            _memoryCache = memoryCache;
            _signal = signal;
            _session = session;
        }

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        /// <inheritdoc/>
        public async Task<BackgroundTaskDocument> GetDocumentAsync()
        {
            if (!_memoryCache.TryGetValue<BackgroundTaskDocument>(CacheKey, out var document))
            {
                document = await _session.Query<BackgroundTaskDocument>().FirstOrDefaultAsync();

                if (document == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(CacheKey, out document))
                        {
                            document = new BackgroundTaskDocument();

                            _session.Save(document);
                            _memoryCache.Set(CacheKey, document, ChangeToken);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(CacheKey, document, ChangeToken);
                }
            }

            return document;
        }

        public async Task RemoveAsync(string name)
        {
            var document = await GetDocumentAsync();

            document.Settings.Remove(name);
            _session.Save(document);

            _signal.SignalToken(CacheKey);
            _memoryCache.Set(CacheKey, document, ChangeToken);
        }
        
        public async Task UpdateAsync(string name, BackgroundTaskSettings settings)
        {
            var document = await GetDocumentAsync();

            document.Settings[name] = settings;
            _session.Save(document);

            _signal.SignalToken(CacheKey);
            _memoryCache.Set(CacheKey, document, ChangeToken);
        }
    }
}
