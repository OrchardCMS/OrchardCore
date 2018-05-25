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
        private const string CacheKey = "UserTimeZone";
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

        public async Task UpdateUserTimeZoneAsync(UserTimeZone userTimeZone)
        {
            if (!String.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.Name))
            {
                var user = await _userManager.FindByNameAsync(_httpContextAccessor.HttpContext.User.Identity.Name) as User;

                _memoryCache.Set(CacheKey, (string)userTimeZone.TimeZoneId, SlidingExpiration);
                _session.Save(user);
            }
        }

        public async Task<string> GetCurrentUserTimeZoneIdAsync()
        {
            if (!String.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.Name))
            {
                if (!_memoryCache.TryGetValue(CacheKey, out string timeZoneId))
                {
                    var user = await _userManager.FindByNameAsync(_httpContextAccessor.HttpContext.User.Identity.Name) as User;

                    if (user.As<UserTimeZone>().TimeZoneId != null)
                    {
                        timeZoneId = user.As<UserTimeZone>().TimeZoneId;
                    }

                    _memoryCache.Set(CacheKey, timeZoneId, SlidingExpiration);
                }

                return timeZoneId;
            }
            else return null;
        }
    }
}
