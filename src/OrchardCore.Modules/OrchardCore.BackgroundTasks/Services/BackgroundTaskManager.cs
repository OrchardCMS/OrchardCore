using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public IDictionary<string, BackgroundTaskAttribute> GetAttributes()
        {
            if (!_memoryCache.TryGetValue<IDictionary<string, BackgroundTaskAttribute>>(
                nameof(BackgroundTaskAttribute), out var attributes))
            {
                if (attributes == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(nameof(BackgroundTaskAttribute), out attributes))
                        {
                            attributes = new Dictionary<string, BackgroundTaskAttribute>();

                            foreach (var task in _backgroundTasks)
                            {
                                var name = task.GetType().FullName;
                                var attribute = task.GetType().GetCustomAttribute<BackgroundTaskAttribute>();
                                attributes[name] = attribute;
                            }
                        }
                    }
                }
            }

            return attributes;
        }

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
