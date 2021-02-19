using System;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Twitter.Signin.Settings;

namespace OrchardCore.Twitter.Signin.Services
{
    public class TwitterSigninService : ITwitterSigninService
    {
        private readonly ISiteService _siteService;

        public TwitterSigninService(
            ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<TwitterSigninSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<TwitterSigninSettings>();
        }

        public async Task<TwitterSigninSettings> LoadSettingsAsync()
        {
            var container = await _siteService.LoadSiteSettingsAsync();
            return container.As<TwitterSigninSettings>();
        }

        public async Task UpdateSettingsAsync(TwitterSigninSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.LoadSiteSettingsAsync();
            container.Alter<TwitterSigninSettings>(nameof(TwitterSigninSettings), aspect =>
            {
                aspect.CallbackPath = settings.CallbackPath;
            });

            await _siteService.UpdateSiteSettingsAsync(container);
        }
    }
}
