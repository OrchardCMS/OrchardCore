using System.Security.Cryptography;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Processing;
using OrchardCore.Media.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Media
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;

        public Migrations(IContentDefinitionManager contentDefinitionManager, ISiteService siteService)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
        }

        // This migration does not need to run on new installations, but because there is no
        // initial migration record, there is no way to shortcut the Create migration.
        public int Create()
        {
            _contentDefinitionManager.MigrateFieldSettings<MediaField, MediaFieldSettings>();

            return 1;
        }

        public async Task<int> UpdateFrom1Async()
        {
            var siteSettings = await _siteService.LoadSiteSettingsAsync();
            var mediaTokenSettings = siteSettings.As<MediaTokenSettings>();

            var rng = RandomNumberGenerator.Create();

            mediaTokenSettings.HashKey = new byte[64];
            rng.GetBytes(mediaTokenSettings.HashKey);
            siteSettings.Put(mediaTokenSettings);

            await _siteService.UpdateSiteSettingsAsync(siteSettings);

            return 2;
        }
    }
}
