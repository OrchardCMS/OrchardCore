using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Settings.Services
{
    /// <summary>
    /// Implements <see cref="ISiteService"/> by storing the site as a Content Item.
    /// </summary>
    public class SiteService : ISiteService
    {
        private const string SiteCacheKey = "SiteService";

        private readonly ISignal _signal;
        private readonly IMemoryCache _memoryCache;
        private readonly IClock _clock;

        public SiteService(
            ISignal signal,
            IMemoryCache memoryCache,
            IClock clock)
        {
            _signal = signal;
            _memoryCache = memoryCache;
            _clock = clock;
        }

        /// <inheritdoc/>
        public IChangeToken ChangeToken => _signal.GetToken(SiteCacheKey);

        /// <summary>
        /// Returns the document from the database to be updated.
        /// </summary>
        public async Task<ISite> LoadSiteSettingsAsync()
        {
            return await SessionHelper.LoadForUpdateAsync(GetDefaultSettings);
        }

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        public async Task<ISite> GetSiteSettingsAsync()
        {
            if (!_memoryCache.TryGetValue<SiteSettings>(SiteCacheKey, out var site))
            {
                var sessionHelper = SessionHelper;

                // First get a new token.
                var changeToken = ChangeToken;

                // The cache is always updated with the actual persisted data.
                site = await sessionHelper.GetForCachingAsync(GetDefaultSettings);

                // Prevent data from being updated.
                site.IsReadonly = true;

                _memoryCache.Set(SiteCacheKey, site, changeToken);
            }

            return site;
        }

        private SiteSettings GetDefaultSettings()
        {
            return new SiteSettings
            {
                SiteSalt = Guid.NewGuid().ToString("N"),
                SiteName = "My Orchard Project Application",
                PageTitleFormat = "{% page_title Site.SiteName, position: \"after\", separator: \" - \" %}",
                TimeZoneId = _clock.GetSystemTimeZone().TimeZoneId,
                PageSize = 10,
                MaxPageSize = 100,
                MaxPagedCount = 0
            };
        }

        /// <inheritdoc/>
        public Task UpdateSiteSettingsAsync(ISite site)
        {
            if (site.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            // Persists new data.
            Session.Save(site);

            // Invalidates the cache after the session is committed.
            _signal.DeferredSignalToken(SiteCacheKey);

            return Task.CompletedTask;
        }

        private ISession Session => ShellScope.Services.GetRequiredService<ISession>();
        private ISessionHelper SessionHelper => ShellScope.Services.GetRequiredService<ISessionHelper>();
    }
}
