using OrchardCore.Entities;
using System.Threading.Tasks;
using OrchardCore.Settings;

namespace OrchardCore.Lucene
{
    public class LuceneAnalyzerSettingsService
    {
        private readonly ISiteService _siteService;
        public LuceneAnalyzerSettingsService(
            ISiteService siteService)
        {
            _siteService = siteService;
        }
        public async Task<LuceneAnalyzerSettings> GetLuceneAnalyzerSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            if (siteSettings.Has<LuceneAnalyzerSettings>())
            {
                return siteSettings.As<LuceneAnalyzerSettings>();
            }

            return new LuceneAnalyzerSettings
            {
                Analyzer = DefaultLuceneAnalyzerSettings.StandardAnalyzer,
                Version = DefaultLuceneAnalyzerSettings.DefaultVersion
            };
        }
    }
}
