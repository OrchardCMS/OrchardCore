using Microsoft.AspNetCore.Http;
using Orchard.DisplayManagement.Theming;
using System;
using System.Threading.Tasks;

namespace Orchard.UserCenter
{
    /// <summary>
    /// Provides the theme defined in the site configuration for the current scope (request).
    /// The same <see cref="ThemeSelectorResult"/> is returned if called multiple times
    /// during the same scope.
    /// </summary>
    public class UserCenterThemeSelector : IThemeSelector
    {
        private readonly IUserCenterThemeService _userCenterThemeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserCenterThemeSelector(
            IUserCenterThemeService userCenterThemeService,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _userCenterThemeService = userCenterThemeService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ThemeSelectorResult> GetThemeAsync()
        {
            if (UserCenterAttribute.IsApplied(_httpContextAccessor.HttpContext))
            {
                string userCenterThemeName = await _userCenterThemeService.GetUserCenterThemeNameAsync();
                if (String.IsNullOrEmpty(userCenterThemeName))
                {
                    return null;
                }

                return new ThemeSelectorResult
                {
                    Priority = 100,
                    ThemeName = userCenterThemeName
                };
            }

            return null;
        }
    }
}
