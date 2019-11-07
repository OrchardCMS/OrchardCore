using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;
using OrchardCore.Templates.Models;
using YesSql;

namespace OrchardCore.Templates.Services
{
    public class AdminTemplatesManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly ISession _session;

        private const string CacheKey = nameof(AdminTemplatesManager);

        public AdminTemplatesManager(IMemoryCache memoryCache, ISignal signal, ISession session)
        {
            _memoryCache = memoryCache;
            _signal = signal;
            _session = session;
        }

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        /// <inheritdoc/>
        public async Task<AdminTemplatesDocument> GetTemplatesDocumentAsync()
        {
            AdminTemplatesDocument document;

            if (!_memoryCache.TryGetValue(CacheKey, out document))
            {
                document = await _session.Query<AdminTemplatesDocument>().FirstOrDefaultAsync();

                if (document == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(CacheKey, out document))
                        {
                            document = new AdminTemplatesDocument();

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

        public async Task RemoveTemplateAsync(string name)
        {
            var document = await GetTemplatesDocumentAsync();

            document.Templates.Remove(name);
            _session.Save(document);

            _memoryCache.Set(CacheKey, document);
            _signal.SignalToken(CacheKey);
        }
        
        public async Task UpdateTemplateAsync(string name, Template template)
        {
            var document = await GetTemplatesDocumentAsync();

            document.Templates[name] = template;
            _session.Save(document);

            _memoryCache.Set(CacheKey, document);
            _signal.SignalToken(CacheKey);
        }
    }
}
