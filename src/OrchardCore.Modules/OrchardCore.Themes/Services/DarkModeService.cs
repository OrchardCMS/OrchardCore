using Microsoft.AspNetCore.Http;
using OrchardCore.Admin.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System.Text.Json;
using System.Threading.Tasks;

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

        public string MediaDefault { get; set; } = "all";

        public string MediaDark { get; set; } = "not all";

        public async Task<bool> IsDarkModeAsync()
        {
            var result = false;
            var adminSettings = (await _siteService.GetSiteSettingsAsync()).As<AdminSettings>();

            if(adminSettings.DisplayDarkMode)
            {
                var adminPreferences = JsonDocument.Parse(_httpContextAccessor.HttpContext.Request.Cookies["adminPreferences"]);

                if (adminPreferences.RootElement.TryGetProperty("darkMode", out var darkMode))
                {
                    result = darkMode.GetBoolean();
                }
            }

            if(result)
            {
                MediaDefault = "not all";
                MediaDark = "all";
            }

            return result;
        }
    }
}
