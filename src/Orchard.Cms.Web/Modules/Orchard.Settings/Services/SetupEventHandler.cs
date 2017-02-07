using System.Threading.Tasks;
using Orchard.Events;

namespace Orchard.Settings.Services
{
    public interface ISetupEventHandler : IEventHandler
    {
        Task Setup(string siteName, string userName);
    }

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

        public async Task Setup(string siteName, string userName)
        {
            // Updating site settings
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            siteSettings.SiteName = siteName;
            siteSettings.SuperUser = userName;
            await _siteService.UpdateSiteSettingsAsync(siteSettings);

            // TODO: Add Encryption Settings in
        }
    }
}
