using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Entities;
using OrchardCore.SecureSocketsLayer.Models;
using OrchardCore.Settings;

namespace OrchardCore.SecureSocketsLayer.Services
{
    public class SecureSocketsLayerService : ISecureSocketsLayerService
    {
        private readonly ISiteService _siteService;

        public SecureSocketsLayerService(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<SslSettings> GetSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync().ConfigureAwait(false);
            var sslSettings = siteSettings.As<SslSettings>();
            return sslSettings ?? new SslSettings();
        }

        public async Task<bool> ShouldBeSecureAsync(AuthorizationFilterContext filterContext)
        {
            var settings = await GetSettingsAsync().ConfigureAwait(false);

            return settings.RequireHttps;
        }
    }
}