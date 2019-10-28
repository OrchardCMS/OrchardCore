using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using OrchardCore.Templates.Models;
using YesSql;

namespace OrchardCore.Templates.Services
{
    public class TemplatesManager
    {
        private const string CacheKey = nameof(TemplatesManager);

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMemoryCache _memoryCache;

        public TemplatesManager(
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
        public Task<TemplatesDocument> LoadTemplatesDocumentAsync() => _sessionHelper.LoadForUpdateAsync<TemplatesDocument>();

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        public async Task<TemplatesDocument> GetTemplatesDocumentAsync()
        {
            if (!_memoryCache.TryGetValue<TemplatesDocument>(CacheKey, out var document))
            {
                var changeToken = ChangeToken;

                document = await _sessionHelper.GetForCachingAsync<TemplatesDocument>();

                foreach (var template in document.Templates.Values)
                {
                    template.IsReadonly = true;
                }

                _memoryCache.Set(CacheKey, document, changeToken);
            }

            return document;
        }

        public async Task RemoveTemplateAsync(string name)
        {
            var document = await LoadTemplatesDocumentAsync();
            document.Templates.Remove(name);

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }

        public async Task UpdateTemplateAsync(string name, Template template)
        {
            if (template.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var document = await LoadTemplatesDocumentAsync();
            document.Templates[name] = template;

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }
    }
}
