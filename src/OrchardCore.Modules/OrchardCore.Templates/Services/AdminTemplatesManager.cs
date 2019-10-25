using System;
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
        private const string CacheKey = nameof(AdminTemplatesManager);

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly IMemoryCache _memoryCache;

        private AdminTemplatesDocument _templatesDocument;

        public AdminTemplatesManager(IMemoryCache memoryCache, ISignal signal, ISession session)
        {
            _memoryCache = memoryCache;
            _signal = signal;
            _session = session;
        }

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        /// <summary>
        /// Returns the document from the database to be updated.
        /// </summary>
        public async Task<AdminTemplatesDocument> LoadTemplatesDocumentAsync()
        {
            return _templatesDocument = _templatesDocument ?? await _session.Query<AdminTemplatesDocument>().FirstOrDefaultAsync() ?? new AdminTemplatesDocument();
        }

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        public async Task<AdminTemplatesDocument> GetTemplatesDocumentAsync()
        {
            AdminTemplatesDocument document;

            if (!_memoryCache.TryGetValue(CacheKey, out document))
            {
                var changeToken = ChangeToken;

                if (_templatesDocument != null)
                {
                    _session.Detach(_templatesDocument);
                }

                document = await _session.Query<AdminTemplatesDocument>().FirstOrDefaultAsync();

                if (document != null)
                {
                    _session.Detach(document);

                    foreach (var template in document.Templates.Values)
                    {
                        template.IsReadonly = true;
                    }
                }
                else
                {
                    document = new AdminTemplatesDocument();
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
