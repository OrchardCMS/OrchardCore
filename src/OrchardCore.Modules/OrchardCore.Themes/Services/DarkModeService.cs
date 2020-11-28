using System;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
            var siteSettings = (await _siteService.GetSiteSettingsAsync());

            siteSettings.Properties.TryGetValue("AdminSettings", out var value);

            if (value != null)
            {
                dynamic adminSettings = JsonConvert.DeserializeObject<ExpandoObject>(value.ToString(), new ExpandoObjectConverter());

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
            }

            if (result)
            {
                CurrentTheme = "darkmode";
            }

            return result;
        }
    }
}
