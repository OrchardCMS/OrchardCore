using System.Threading.Tasks;
using OrchardCore.Autoroute.Services;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.ContentLocalization
{
    public class ContentCulturePickerService : IContentCulturePickerService
    {
        private readonly ISession _session;
        private readonly IAutorouteEntries _autorouteEntries;
        private readonly ISiteService _siteService;
        private readonly IContentLocalizationManager _contentLocalizationManager;
        private readonly string _tenantPrefix;


        public ContentCulturePickerService(
            IContentLocalizationManager contentLocalizationManager,
            ISession session,

            IAutorouteEntries autorouteEntries,
            ShellSettings shellSettings,
            ISiteService siteService
            )
        {
            _contentLocalizationManager = contentLocalizationManager;
            _session = session;
            _autorouteEntries = autorouteEntries;
            _siteService = siteService;
            _tenantPrefix = "/" + shellSettings.RequestUrlPrefix;
        }

        public async Task<string> GetContentItemIdFromRoute(string url)
        {
            var cleanedUrl = url.Remove(url.IndexOf(_tenantPrefix), _tenantPrefix.Length);
            string contentItemId;
            if (cleanedUrl == "/")
            {
                // get contentItemId from homeroute
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                contentItemId = siteSettings.HomeRoute["contentItemId"]?.ToString();

            }
            else
            {
                // try to get from autorouteEntries
                _autorouteEntries.TryGetContentItemId(cleanedUrl, out contentItemId);
            }
            if (string.IsNullOrEmpty(contentItemId))
            {
                return null;
            }

            return contentItemId;
        }

        public async Task<string> GetCultureFromRoute(string url)
        {
            var contentItemId = await GetContentItemIdFromRoute(url);
            if (!string.IsNullOrEmpty(contentItemId))
            {
                var indexValue = await _session.QueryIndex<LocalizedContentItemIndex>(o => o.ContentItemId == contentItemId).FirstOrDefaultAsync();
                if (indexValue is object)
                {
                    return indexValue.Culture;
                }
            }
            return null;
        }

        public async Task<ContentItem> GetRelatedContentItem(string contentItemId, string culture)
        {
            var indexValue = await _session.QueryIndex<LocalizedContentItemIndex>(o =>
                o.ContentItemId == contentItemId
            ).FirstOrDefaultAsync();

            if (indexValue is object)
            {
                return await _contentLocalizationManager.GetContentItem(indexValue.LocalizationSet, culture);
            }
            return null;
        }
    }
}
