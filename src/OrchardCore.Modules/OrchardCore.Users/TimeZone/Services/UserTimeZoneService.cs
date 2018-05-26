using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Models;

namespace OrchardCore.Users.TimeZone.Services
{
    public class UserTimeZoneService
    {
        private const string CacheKey = "UserTimeZone/";
        private readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(1);

        private readonly IClock _clock;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IUser> _userManager;
        private readonly YesSql.ISession _session;

        public UserTimeZoneService(
            IClock clock,
            IMemoryCache memoryCache,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IUser> userManager,
            YesSql.ISession session
            )
        {
            _clock = clock;
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _session = session;
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

        public Task UpdateUserTimeZoneAsync(User user)
        {
            var userName = user?.UserName;

            if (!String.IsNullOrEmpty(userName))
            {
                _memoryCache.Remove(GetCacheKey(userName));
            }

            return Task.CompletedTask;
        }

        public async Task<string> GetCurrentUserTimeZoneIdAsync()
        {
            var userName = _httpContextAccessor.HttpContext.User?.Identity?.Name;

            if (String.IsNullOrEmpty(userName))
            {
                return null;
            }

            var key = GetCacheKey(userName);

            if (!_memoryCache.TryGetValue(key, out string timeZoneId))
            {
                var user = await _userManager.FindByNameAsync(userName) as User;
                timeZoneId = user.As<UserTimeZone>()?.TimeZoneId;
                
                if (!String.IsNullOrEmpty(timeZoneId))
                {
                    _memoryCache.Set(key, timeZoneId, SlidingExpiration);
                    return timeZoneId;
                }                
            }
            else
            {
                return timeZoneId;
            }

            return null;
        }

        private string GetCacheKey(string userName) => CacheKey + userName;
    }
}
