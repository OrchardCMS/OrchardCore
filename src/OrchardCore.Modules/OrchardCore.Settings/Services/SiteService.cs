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
            ISignal signal,
            IMemoryCache memoryCache,
            IClock clock)
        {
            _signal = signal;
            _clock = clock;
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
                var changeToken = ChangeToken;
                site = await Session.Query<SiteSettings>().FirstOrDefaultAsync();

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

                    await SaveAsync(site);
                }
                else
                {
                    _memoryCache.Set(SiteCacheKey, site, changeToken);
                }
            }

            return site;
        }

        /// <inheritdoc/>
        public async Task UpdateSiteSettingsAsync(ISite site)
        {
            var existing = await GetSiteSettingsAsync() as SiteSettings;

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

            await SaveAsync(existing);

            return;
        }

        private async Task SaveAsync(ISite site)
        {
            var session = Session;

            session.Save(site);
            await session.CommitAsync();
            _signal.SignalToken(SiteCacheKey);
        }

        private ISession Session => ShellScope.Services.GetService<ISession>();
    }
}
