using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using OrchardCore.Media.Models;
using YesSql;

namespace OrchardCore.Media.Services
{
    public class MediaProfilesManager
    {
        private const string CacheKey = nameof(MediaProfilesManager);

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMemoryCache _memoryCache;

        public MediaProfilesManager(
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
        public Task<MediaProfilesDocument> LoadMediaProfilesDocumentAsync() => _sessionHelper.LoadForUpdateAsync<MediaProfilesDocument>();

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        public async Task<MediaProfilesDocument> GetMediaProfilesDocumentAsync()
        {
            if (!_memoryCache.TryGetValue<MediaProfilesDocument>(CacheKey, out var document))
            {
                var changeToken = ChangeToken;
                bool cacheable;

                (cacheable, document) = await _sessionHelper.GetForCachingAsync<MediaProfilesDocument>();

                if (cacheable)
                {
                    foreach (var template in document.MediaProfiles.Values)
                    {
                        template.IsReadonly = true;
                    }

                    _memoryCache.Set(CacheKey, document, changeToken);
                }
            }

            return document;
        }

        public async Task RemoveMediaProfileAsync(string name)
        {
            var document = await LoadMediaProfilesDocumentAsync();
            document.MediaProfiles.Remove(name);

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }

        public async Task UpdateMediaProfileAsync(string name, MediaProfile mediaProfile)
        {
            if (mediaProfile.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var document = await LoadMediaProfilesDocumentAsync();
            document.MediaProfiles[name.ToLower()] = mediaProfile;

            _session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }
    }
}
