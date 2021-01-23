using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            siteSettings.SiteName = properties["SiteName"]?.ToString();
            siteSettings.SuperUser = properties["AdminUserId"]?.ToString();
            siteSettings.TimeZoneId = properties["SiteTimeZone"]?.ToString();
            await _siteService.UpdateSiteSettingsAsync(siteSettings);

            // TODO: Add Encryption Settings in
        }
    }
}
