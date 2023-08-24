using System;
using System.Threading.Tasks;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Setup.Events;
using OrchardCore.Setup.Services;

namespace OrchardCore.Settings.Services
{
    /// <summary>
    /// During setup, registers the Super User.
    /// </summary>
    public class SetupEventHandler : ISetupEventHandler
    {
        private readonly ISiteService _siteService;

        public SetupEventHandler(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task SetupAsync(SetupContext context)
        {
            // Updating site settings.
            var siteSettings = await _siteService.LoadSiteSettingsAsync();
            siteSettings.SiteName = context.Properties.TryGetValue(SetupConstants.SiteName, out var siteName) ? siteName?.ToString() : String.Empty;
            siteSettings.SuperUser = context.Properties.TryGetValue(SetupConstants.AdminUserId, out var adminUserId) ? adminUserId?.ToString() : String.Empty;
            siteSettings.TimeZoneId = context.Properties.TryGetValue(SetupConstants.SiteTimeZone, out var siteTimeZone) ? siteTimeZone?.ToString() : String.Empty;

            await _siteService.UpdateSiteSettingsAsync(siteSettings);

            // TODO: Add Encryption Settings in.
        }
    }
}
