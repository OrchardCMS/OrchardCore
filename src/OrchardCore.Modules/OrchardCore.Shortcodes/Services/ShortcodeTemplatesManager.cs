using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using OrchardCore.Shortcodes.Models;
using YesSql;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeTemplatesManager
    {
        private const string CacheKey = nameof(ShortcodeTemplatesManager);

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMemoryCache _memoryCache;

        public ShortcodeTemplatesManager(
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
        public Task<ShortcodeTemplatesDocument> LoadShortcodeTemplatesDocumentAsync() => _sessionHelper.LoadForUpdateAsync<ShortcodeTemplatesDocument>();

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        public async Task<ShortcodeTemplatesDocument> GetShortcodeTemplatesDocumentAsync()
        {
            if (!_memoryCache.TryGetValue<ShortcodeTemplatesDocument>(CacheKey, out var document))
            {
                var changeToken = ChangeToken;
                bool cacheable;

                (cacheable, document) = await _sessionHelper.GetForCachingAsync<ShortcodeTemplatesDocument>();

                if (cacheable)
                {
                    foreach (var template in document.ShortcodeTemplates.Values)
                    {
                        template.IsReadonly = true;
                    }

                    _memoryCache.Set(CacheKey, document, changeToken);
                }
            }

            return document;
        }

        public async Task RemoveShortcodeTemplateAsync(string name)
        {
            var document = await LoadShortcodeTemplatesDocumentAsync();
            document.ShortcodeTemplates.Remove(name);

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }

        public async Task UpdateShortcodeTemplateAsync(string name, ShortcodeTemplate template)
        {
            if (template.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var document = await LoadShortcodeTemplatesDocumentAsync();
            document.ShortcodeTemplates[name.ToLower()] = template;

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }
    }
}
