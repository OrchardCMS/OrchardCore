using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        private readonly YesSql.ISession _session;
        private readonly IAutorouteEntries _autorouteEntries;
        private readonly ISiteService _siteService;
        private readonly IContentLocalizationManager _contentLocalizationManager;


        public ContentCulturePickerService(
            IContentLocalizationManager contentLocalizationManager,
            YesSql.ISession session,
            IAutorouteEntries autorouteEntries,
            ShellSettings shellSettings,
            ISiteService siteService
            )
        {
            _contentLocalizationManager = contentLocalizationManager;
            _session = session;
            _autorouteEntries = autorouteEntries;
            _siteService = siteService;
        }

        public async Task<string> GetContentItemIdFromRoute(PathString url)
        {
            if (!url.HasValue)
            {
                url = "/";
            }

            string contentItemId;

            if (url == "/")
            {
                // get contentItemId from homeroute
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                contentItemId = siteSettings.HomeRoute["contentItemId"]?.ToString();

            }
            else
            {
                // try to get from autorouteEntries
                _autorouteEntries.TryGetContentItemId(url, out contentItemId);
            }

            if (string.IsNullOrEmpty(contentItemId))
            {
                return null;
            }

            return contentItemId;
        }

        public async Task<string> GetCultureFromRoute(PathString url)
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
            var indexValue = await _session.QueryIndex<LocalizedContentItemIndex>(o => o.ContentItemId == contentItemId)
                .FirstOrDefaultAsync();

            if (indexValue is object)
            {
                return await _contentLocalizationManager.GetContentItem(indexValue.LocalizationSet, culture);
            }

            return null;
        }
    }
}
