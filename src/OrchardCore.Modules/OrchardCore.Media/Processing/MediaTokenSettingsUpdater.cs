using System.Security.Cryptography;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// Generates a random key for media token hashing.
    /// To regenerate the key disabled and enable the feature.
    /// </summary>
    public class MediaTokenSettingsUpdater : ModularTenantEvents, IFeatureEventHandler
    {
        private readonly ISiteService _siteService;
        private readonly ShellSettings _shellSettings;

        public MediaTokenSettingsUpdater(ISiteService siteService, ShellSettings shellSettings)
        {
            _siteService = siteService;
            _shellSettings = shellSettings;
        }

        public override async Task ActivatedAsync()
        {

            if (_shellSettings.State == TenantState.Uninitialized)
            {
                // If the tenant is 'Uninitialized' there is no registered 'ISession' and then 'ISiteService' can't be used.
                return;
            }

            var mediaTokenSettings = (await _siteService.GetSiteSettingsAsync()).As<MediaTokenSettings>();

            if (mediaTokenSettings.HashKey == null)
            {
                var siteSettings = await _siteService.LoadSiteSettingsAsync();

                var rng = RandomNumberGenerator.Create();

                mediaTokenSettings.HashKey = new byte[64];
                rng.GetBytes(mediaTokenSettings.HashKey);
                siteSettings.Put(mediaTokenSettings);

                await _siteService.UpdateSiteSettingsAsync(siteSettings);
            }
        }

        Task IFeatureEventHandler.InstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.InstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature) => SetMediaTokenSettingsAsync(feature);

        Task IFeatureEventHandler.DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.DisabledAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

        private async Task SetMediaTokenSettingsAsync(IFeatureInfo feature)
        {
            if (feature.Extension.Id != GetType().Assembly.GetName().Name)
            {
                return;
            }

            var siteSettings = await _siteService.LoadSiteSettingsAsync();
            var mediaTokenSettings = siteSettings.As<MediaTokenSettings>();

            var rng = RandomNumberGenerator.Create();

            mediaTokenSettings.HashKey = new byte[64];
            rng.GetBytes(mediaTokenSettings.HashKey);
            siteSettings.Put(mediaTokenSettings);

            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }
    }
}
