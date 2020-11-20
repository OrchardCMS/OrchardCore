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
            dynamic adminPreferences = JsonDocument.Parse(_httpContextAccessor.HttpContext.Request.Cookies["adminPreferences"]);
            var darkMode = adminPreferences.DarkMode;
            return false;
        }
    }
}
