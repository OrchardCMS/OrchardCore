using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin.Models;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Themes.Services
{
    public class DarkModeService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISiteService _siteService;

        public DarkModeService(
            IHttpContextAccessor httpContextAccessor,
            ISiteService siteService,
            ShellSettings shellSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _siteService = siteService;
            CurrentTenant = shellSettings.Name;
        }

        public string CurrentTenant { get; }

        public string CurrentTheme { get; set; } = "default";

        public async Task<bool> IsDarkModeAsync()
        {
            var result = false;
            var adminSettings = (await _siteService.GetSiteSettingsAsync()).As<AdminSettings>();
            var cookieName = $"{CurrentTenant}-adminPreferences";

            if (adminSettings.DisplayDarkMode)
            {
                if (!String.IsNullOrWhiteSpace(_httpContextAccessor.HttpContext.Request.Cookies[cookieName]))
                {
                    var adminPreferences = JsonDocument.Parse(_httpContextAccessor.HttpContext.Request.Cookies[cookieName]);

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
