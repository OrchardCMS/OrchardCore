using OrchardCore.Entities;
using System.Threading.Tasks;
using OrchardCore.Settings;

namespace OrchardCore.Lucene
{
    public class LuceneSettingsService
    {
        private readonly ISiteService _siteService;
        public LuceneSettingsService(
            ISiteService siteService)
        {
            _siteService = siteService;
        }
        public async Task<LuceneSettings> GetLuceneSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            if (siteSettings.Has<LuceneSettings>())
            {
                return siteSettings.As<LuceneSettings>();
            }

            return null;
        }
    }
}
