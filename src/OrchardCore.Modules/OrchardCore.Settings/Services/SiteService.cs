using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
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
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IClock _clock;

        private const string SiteCacheKey = "SiteService";

        public SiteService(
            IMemoryCache memoryCache,
            ISignal signal,
            IClock clock)
        {
            _memoryCache = memoryCache;
            _signal = signal;
            _clock = clock;
        }

        /// <inheritdoc/>
        public IChangeToken ChangeToken => _signal.GetToken(SiteCacheKey);

        private SiteSettingsCache ScopedCache => ShellScope.Services.GetRequiredService<SiteSettingsCache>();

        /// <inheritdoc/>
        public async Task<ISite> GetSiteSettingsAsync()
        {
            var scopedCache = ScopedCache;

            if (scopedCache.SiteSettings != null)
            {
                return scopedCache.SiteSettings;
            }

            SiteSettings site;

            if (!_memoryCache.TryGetValue(SiteCacheKey, out site))
            {
                var session = Session;

                var changeToken = ChangeToken;
                site = await session.Query<SiteSettings>().FirstOrDefaultAsync();

                if (site == null)
                {
                    site = new SiteSettings
                    {
                        SiteSalt = Guid.NewGuid().ToString("N"),
                        SiteName = "My Orchard Project Application",
                        PageSize = 10,
                        MaxPageSize = 100,
                        MaxPagedCount = 0,
                        TimeZoneId = _clock.GetSystemTimeZone().TimeZoneId,
                    };

                    session.Save(site);
                    _signal.SignalToken(SiteCacheKey);
                }
                else
                {
                    _memoryCache.Set(SiteCacheKey, site.Clone(), changeToken);
                }

                return scopedCache.SiteSettings = site;
            }

            return scopedCache.SiteSettings = site.Clone();
        }

        /// <inheritdoc/>
        public Task UpdateSiteSettingsAsync(ISite site)
        {
            var existing = ScopedCache.SiteSettings;

            existing.BaseUrl = site.BaseUrl;
            existing.Calendar = site.Calendar;
            existing.HomeRoute = site.HomeRoute;
            existing.MaxPagedCount = site.MaxPagedCount;
            existing.MaxPageSize = site.MaxPageSize;
            existing.PageSize = site.PageSize;
            existing.Properties = site.Properties;
            existing.ResourceDebugMode = site.ResourceDebugMode;
            existing.SiteName = site.SiteName;
            existing.SiteSalt = site.SiteSalt;
            existing.SuperUser = site.SuperUser;
            existing.TimeZoneId = site.TimeZoneId;
            existing.UseCdn = site.UseCdn;
            existing.CdnBaseUrl = site.CdnBaseUrl;
            existing.AppendVersion = site.AppendVersion;

            Session.Save(existing);
            _signal.SignalToken(SiteCacheKey);

            return Task.CompletedTask;
        }

        private ISession Session => ShellScope.Services.GetService<ISession>();
    }
}
