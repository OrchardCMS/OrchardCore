using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace OrchardCore.Themes.Services
{
    public class DarkModeService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DarkModeService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsDarkMode()
        {
            var adminPreferences = JsonDocument.Parse(_httpContextAccessor.HttpContext.Request.Cookies["adminPreferences"]);
            var result = false;

            if(adminPreferences.RootElement.TryGetProperty("darkMode", out var darkMode))
            {
                result = darkMode.GetBoolean();
            }

            return result;
        }
    }
}
