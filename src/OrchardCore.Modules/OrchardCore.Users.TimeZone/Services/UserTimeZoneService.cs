using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Models;
using YesSql;

namespace OrchardCore.Users.TimeZone.Services
{
    public class UserTimeZoneService : IUserTimeZoneService
    {
        private const string CacheKey = "UserTimeZone";

        private readonly IClock _clock;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserTimeZoneService(
            IClock clock,
            IMemoryCache memoryCache,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _clock = clock;
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ITimeZone> GetUserTimeZoneAsync()
        {
            string currentTimeZoneId = await GetCurrentUserTimeZoneIdAsync();
            if (String.IsNullOrEmpty(currentTimeZoneId))
            {
                return null;
            }

            return _clock.GetTimeZone(currentTimeZoneId);
        }

        public async Task UpdateUserTimeZoneAsync(UserProfile profile)
        {
            var session = GetSession();

            var user = await session.Query<User, UserIndex>().Where(x => x.NormalizedUserName == _httpContextAccessor.HttpContext.User.Identity.Name.ToUpper()).FirstOrDefaultAsync();
            if (user.Properties["UserProfile"] != null && user.Properties["UserProfile"]["TimeZone"] != null)
            {
                user.Properties["UserProfile"]["TimeZone"] = profile.TimeZone;
            }

            _memoryCache.Set(CacheKey, (string)profile.TimeZone);
            session.Save(user);
        }

        public async Task<string> GetCurrentUserTimeZoneIdAsync()
        {
            if (!_memoryCache.TryGetValue(CacheKey, out string timeZoneId))
            {
                var session = GetSession();

                var user = await session.Query<User, UserIndex>().Where(x => x.NormalizedUserName == _httpContextAccessor.HttpContext.User.Identity.Name.ToUpper()).FirstOrDefaultAsync();
                timeZoneId = (string)user.Properties["TimeZone"] ?? _clock.GetTimeZone(String.Empty).TimeZoneId;

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
