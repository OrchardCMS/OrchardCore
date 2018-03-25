using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.BackgroundTasks.Models;
using OrchardCore.Environment.Cache;
using YesSql;

namespace OrchardCore.BackgroundTasks.Services
{
    public class BackgroundTaskManager
    {
        private readonly IEnumerable<IBackgroundTask> _backgroundTasks;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly ISession _session;

        private const string CacheKey = nameof(BackgroundTaskManager);

        public BackgroundTaskManager(
            IEnumerable<IBackgroundTask> backgroundTasks,
            IMemoryCache memoryCache,
            ISignal signal,
            ISession session)
        {
            _backgroundTasks = backgroundTasks;
            _memoryCache = memoryCache;
            _signal = signal;
            _session = session;
        }

        public IEnumerable<string> Names => _backgroundTasks.Select(t => t.GetType().FullName);

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
                            _memoryCache.Set(CacheKey, document);
                            _signal.SignalToken(CacheKey);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(CacheKey, document);
                    _signal.SignalToken(CacheKey);
                }
            }

            return document;
        }

        public async Task RemoveAsync(string name)
        {
            var document = await GetDocumentAsync();

            document.Tasks.Remove(name);
            _session.Save(document);

            _memoryCache.Set(CacheKey, document);
            _signal.SignalToken(CacheKey);
        }
        
        public async Task UpdateAsync(string name, BackgroundTask task)
        {
            var document = await GetDocumentAsync();

            document.Tasks[name] = task;
            _session.Save(document);

            _memoryCache.Set(CacheKey, document);
            _signal.SignalToken(CacheKey);
        }
    }
}
