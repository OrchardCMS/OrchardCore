using System;
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
            string siteName,
            string userName,
            string userId,
            string email,
            string password,
            string dbProvider,
            string dbConnectionString,
            string dbTablePrefix,
            string siteTimeZone,
            Action<string, string> reportError
            )
        {
            // Updating site settings
            var siteSettings = await _siteService.LoadSiteSettingsAsync();
            siteSettings.SiteName = siteName;
            siteSettings.SuperUser = userId;
            siteSettings.TimeZoneId = siteTimeZone;
            // Disable Cdn by default
            siteSettings.UseCdn = false;

            await _siteService.UpdateSiteSettingsAsync(siteSettings);

            // TODO: Add Encryption Settings in
        }
    }
}
