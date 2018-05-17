using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.TimeZone.Services
{
    public class UserTimeZoneService : IUserTimeZoneService
    {
        private const string CacheKey = "UserTimeZone";

        private readonly IClock _clock;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;

        public UserTimeZoneService(
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

            var user = await session.Query<User>().FirstOrDefaultAsync();

            user.Properties["TimeZone"] = timeZoneId;
            _memoryCache.Set(CacheKey, (string)user.Properties["TimeZone"]);
            session.Save(user);
        }

        public async Task<string> GetCurrentTimeZoneIdAsync()
        {
            if (!_memoryCache.TryGetValue(CacheKey, out string timeZoneId))
            {
                var session = GetSession();

                var user = await session.Query<User>().FirstOrDefaultAsync();
                timeZoneId = (string)user.Properties["TimeZone"] ?? _clock.GetLocalTimeZone(String.Empty).Id;

                _memoryCache.Set(CacheKey, (string)user.Properties["TimeZone"]);
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
