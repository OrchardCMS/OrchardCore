using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Setup.Events;

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

        public async Task Setup(
            IDictionary<string, object> properties,
            Action<string, string> reportError
            )
        {
            // Updating site settings
            var siteSettings = await _siteService.LoadSiteSettingsAsync();
            siteSettings.SiteName = properties.TryGetValue(SetupConstants.SiteName, out var siteName) ? siteName?.ToString() : String.Empty;
            siteSettings.SuperUser = properties.TryGetValue(SetupConstants.AdminUserId, out var adminUserId) ? adminUserId?.ToString() : String.Empty;
            siteSettings.TimeZoneId = properties.TryGetValue(SetupConstants.SiteTimeZone, out var siteTimeZone) ? siteTimeZone?.ToString() : String.Empty;
            await _siteService.UpdateSiteSettingsAsync(siteSettings);

            // TODO: Add Encryption Settings in
        }
    }
}
