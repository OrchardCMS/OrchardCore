using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Settings.Services
{
    public class DefaultTimeZoneService : IDefaultTimeZoneService
    {
        private const string CacheKey = "TimeZone";

        private readonly IClock _clock;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;

        public DefaultTimeZoneService(
            IClock clock,
            IMemoryCache memoryCache,
            IServiceProvider serviceProvider
            )
        {
            _clock = clock;
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;
        }

        public async Task<ITimeZone> GetSiteTimeZoneAsync()
        {
            string currentTimeZoneId = await GetCurrentTimeZoneIdAsync();
            if (String.IsNullOrEmpty(currentTimeZoneId))
            {
                return null;
            }

            return _clock.GetLocalTimeZone(currentTimeZoneId);
        }

        public async Task SetSiteTimeZoneAsync(string timeZoneId)
        {
            var session = GetSession();

            var site = await session.Query<SiteSettings>().FirstOrDefaultAsync() as ISite;

            site.Properties["TimeZone"] = timeZoneId;
            _memoryCache.Set(CacheKey, timeZoneId);
            session.Save(site);
        }

        public async Task<string> GetCurrentTimeZoneIdAsync()
        {
            string timeZoneId;
            if (!_memoryCache.TryGetValue(CacheKey, out timeZoneId))
            {
                var session = GetSession();

                var site = await session.Query<SiteSettings>().FirstOrDefaultAsync() as ISite;
                timeZoneId = (string)site.Properties["TimeZone"];
                _memoryCache.Set(CacheKey, site);
            }

            return timeZoneId;
        }

        private YesSql.ISession GetSession()
        {
            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            return httpContextAccessor.HttpContext.RequestServices.GetService<YesSql.ISession>();
        }
    }
}
