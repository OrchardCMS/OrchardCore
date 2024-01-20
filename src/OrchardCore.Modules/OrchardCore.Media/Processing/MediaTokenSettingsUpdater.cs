using System.Security.Cryptography;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// Generates a random key for media token hashing.
    /// To regenerate the key disabled and enable the feature.
    /// </summary>
    public class MediaTokenSettingsUpdater : FeatureEventHandler, IModularTenantEvents
    {
        private readonly ISiteService _siteService;
        private readonly ShellSettings _shellSettings;

        public MediaTokenSettingsUpdater(ISiteService siteService, ShellSettings shellSettings)
        {
            _siteService = siteService;
            _shellSettings = shellSettings;
        }

        public async Task ActivatedAsync()
        {
            if (_shellSettings.IsUninitialized())
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

        public Task ActivatingAsync() => Task.CompletedTask;
        public Task RemovingAsync(ShellRemovingContext context) => Task.CompletedTask;
        public Task TerminatedAsync() => Task.CompletedTask;
        public Task TerminatingAsync() => Task.CompletedTask;

        public override Task EnabledAsync(IFeatureInfo feature) => SetMediaTokenSettingsAsync(feature);

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
