using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using YesSql.Core.Services;

namespace Orchard.Settings.Services
{
    /// <summary>
    /// Implements <see cref="ISiteService"/> by storing the site as a Content Item.
    /// </summary>
    public class SiteService : ISiteService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;

        private const string SiteCacheKey = "Site";

        public SiteService(
            ISession session,
            IMemoryCache memoryCache)
        {
            _session = session;
            _memoryCache = memoryCache;
        }

        /// <inheritdoc/>
        public async Task<ISite> GetSiteSettingsAsync()
        {
            SiteSettings site;

            if (!_memoryCache.TryGetValue(SiteCacheKey, out site))
            {
                site = await _session.QueryAsync<SiteSettings>().FirstOrDefault();

                if (site == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(SiteCacheKey, out site))
                        {
                            site = new SiteSettings
                            { 
                                SiteSalt = Guid.NewGuid().ToString("N"),
                                SiteName = "My Orchard Project Application",
                                PageTitleSeparator = " - ",
                                TimeZone = TimeZoneInfo.Local.Id,
                                PageSize = 10,
                                MaxPageSize = 100,
                                MaxPagedCount = 500
                            };

                            _session.Save(site);
                            _memoryCache.Set(SiteCacheKey, site);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(SiteCacheKey, site);
                }
            }

            return site;
        }

        /// <inheritdoc/>
        public Task UpdateSiteSettingsAsync(ISite site)
        {
            var siteSettings = site as SiteSettings;
            _session.Save(siteSettings);

            _memoryCache.Set(SiteCacheKey, siteSettings);
            return Task.CompletedTask;
        }
    }
}
