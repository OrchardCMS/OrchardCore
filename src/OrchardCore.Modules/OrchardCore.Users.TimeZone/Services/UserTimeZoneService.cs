using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Models;
using YesSql;

namespace OrchardCore.Users.TimeZone.Services
{
    public class UserTimeZoneService
    {
        private const string CacheKey = "UserTimeZone";

        private readonly IClock _clock;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IUser> _userManager;

        public UserTimeZoneService(
            IClock clock,
            IMemoryCache memoryCache,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IUser> userManager
            )
        {
            _clock = clock;
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
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
            if (!String.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.Name))
            {
                var user = await _userManager.FindByNameAsync(_httpContextAccessor.HttpContext.User.Identity.Name) as User;

                if (user.Properties["UserProfile"] != null && user.Properties["UserProfile"]["TimeZone"] != null)
                {
                    user.Properties["UserProfile"]["TimeZone"] = profile.TimeZoneId;
                }

                _memoryCache.Set(CacheKey, (string)profile.TimeZoneId, new TimeSpan(0, 1, 0));
                await _userManager.UpdateAsync(user);
            }
        }

        public async Task<string> GetCurrentUserTimeZoneIdAsync()
        {
            if (!String.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.Name))
            {
                if (!_memoryCache.TryGetValue(CacheKey, out string timeZoneId))
                {
                    var user = await _userManager.FindByNameAsync(_httpContextAccessor.HttpContext.User.Identity.Name) as User;
                    timeZoneId = (string)user.Properties["UserProfile"]["TimeZone"] ?? _clock.GetSystemTimeZone().TimeZoneId;

                    _memoryCache.Set(CacheKey, (string)user.Properties["UserProfile"]["TimeZone"], new TimeSpan(0, 1, 0));
                }

                return timeZoneId;
            }
            else return null;
        }
    }
}
