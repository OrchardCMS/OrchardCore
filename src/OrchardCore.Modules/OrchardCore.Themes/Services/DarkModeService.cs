using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Themes.Services
{
    public class DarkModeService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISiteService _siteService;

        public DarkModeService(
            IHttpContextAccessor httpContextAccessor,
            ISiteService siteService)
        {
            _httpContextAccessor = httpContextAccessor;
            _siteService = siteService;
        }

        public string CurrentTheme { get; set; } = "default";

        public async Task<bool> IsDarkModeAsync()
        {
            var result = false;
            var adminSettings = (await _siteService.GetSiteSettingsAsync()).As<AdminSettings>();

            if (adminSettings.DisplayDarkMode)
            {
                if (!String.IsNullOrWhiteSpace(_httpContextAccessor.HttpContext.Request.Cookies["adminPreferences"]))
                {
                    var adminPreferences = JsonDocument.Parse(_httpContextAccessor.HttpContext.Request.Cookies["adminPreferences"]);

                    if (adminPreferences.RootElement.TryGetProperty("darkMode", out var darkMode))
                    {
                        result = darkMode.GetBoolean();
                    }
                }
            }

            if (result)
            {
                CurrentTheme = "darkmode";
            }

            return result;
        }
    }
}
