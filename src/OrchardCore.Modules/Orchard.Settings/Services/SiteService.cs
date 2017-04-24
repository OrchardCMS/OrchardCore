using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Orchard.Environment.Cache;
using YesSql;

namespace Orchard.Settings.Services
{
    /// <summary>
    /// Implements <see cref="ISiteService"/> by storing the site as a Content Item.
    /// </summary>
    public class SiteService : ISiteService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;

        private const string SiteCacheKey = "SiteService";
        
        public SiteService(
            ISignal signal,
            ISession session,
            IMemoryCache memoryCache)
        {
            _signal = signal;
            _session = session;
            _memoryCache = memoryCache;
        }

        /// <inheritdoc/>
        public IChangeToken ChangeToken => _signal.GetToken(SiteCacheKey);

        /// <inheritdoc/>
        public async Task<ISite> GetSiteSettingsAsync()
        {
            ISite site;

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
                                TimeZone = TimeZoneInfo.Local.Id,
                                PageSize = 10,
                                MaxPageSize = 100,
                                MaxPagedCount = 0
                            };

                            _session.Save(site);
                            _memoryCache.Set(SiteCacheKey, site);
                            _signal.SignalToken(SiteCacheKey);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(SiteCacheKey, site);
                    _signal.SignalToken(SiteCacheKey);
                }
            }

            return site;
        }

        /// <inheritdoc/>
        public async Task UpdateSiteSettingsAsync(ISite site)
        {
            // Load the currently saved object otherwise it would create a new document
            // as the new session is not tracking the cached object.
            // TODO: Solve by having an Update method in Session

            var existing = await _session.QueryAsync<SiteSettings>().FirstOrDefault();
            
            existing.BaseUrl = site.BaseUrl;
            existing.Calendar = site.Calendar;
            existing.Culture = site.Culture;
            existing.HomeRoute = site.HomeRoute;
            existing.MaxPagedCount = site.MaxPagedCount;
            existing.MaxPageSize = site.MaxPageSize;
            existing.PageSize = site.PageSize;
            existing.Properties = site.Properties;
            existing.ResourceDebugMode = site.ResourceDebugMode;
            existing.SiteName = site.SiteName;
            existing.SiteSalt = site.SiteSalt;
            existing.SuperUser = site.SuperUser;
            existing.TimeZone = site.TimeZone;
            existing.UseCdn = site.UseCdn;

            _session.Save(existing);

            _memoryCache.Set(SiteCacheKey, site);
            _signal.SignalToken(SiteCacheKey);

            return;
        }
    }
}
