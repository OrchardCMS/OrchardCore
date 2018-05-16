using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Settings.Services
{
    /// <summary>
    /// Implements <see cref="ISiteService"/> by storing the site as a Content Item.
    /// </summary>
    public class SiteService : ISiteService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IServiceProvider _serviceProvider;
        private readonly IClock _clock;
        private readonly IDefaultTimeZoneService _siteTimeZoneService;
        private const string SiteCacheKey = "SiteService";

        public SiteService(
            ISignal signal,
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache, IClock clock, IDefaultTimeZoneService siteTimeZoneService)
        {
            _signal = signal;
            _serviceProvider = serviceProvider;
            _clock = clock;
            _memoryCache = memoryCache;
            _siteTimeZoneService = siteTimeZoneService;
        }

        /// <inheritdoc/>
        public IChangeToken ChangeToken => _signal.GetToken(SiteCacheKey);

        /// <inheritdoc/>
        public async Task<ISite> GetSiteSettingsAsync()
        {
            ISite site;

            if (!_memoryCache.TryGetValue(SiteCacheKey, out site))
            {
                var session = GetSession();

                site = await session.Query<SiteSettings>().FirstOrDefaultAsync();

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
                                PageSize = 10,
                                MaxPageSize = 100,
                                MaxPagedCount = 0
                            };

                            site.Properties["TimeZone"] = _clock.GetLocalTimeZone(string.Empty).Id;

                            session.Save(site);
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
            var session = GetSession();

            var existing = await session.Query<SiteSettings>().FirstOrDefaultAsync();
            
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
            existing.UseCdn = site.UseCdn;

            session.Save(existing);

            _memoryCache.Set(SiteCacheKey, site);
            _signal.SignalToken(SiteCacheKey);

            await _siteTimeZoneService.SetSiteTimeZoneAsync((string)site.Properties["TimeZone"]);

            return;
        }

        private YesSql.ISession GetSession()
        {
            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            return httpContextAccessor.HttpContext.RequestServices.GetService<YesSql.ISession>();
        }
    }
}
