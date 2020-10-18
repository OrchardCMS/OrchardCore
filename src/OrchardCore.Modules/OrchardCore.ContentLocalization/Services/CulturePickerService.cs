using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Services
{
    public class ContentCulturePickerService : IContentCulturePickerService
    {
        private readonly IAutorouteEntries _autorouteEntries;
        private readonly ILocalizationEntries _localizationEntries;
        private readonly ISiteService _siteService;

        public ContentCulturePickerService(
            IAutorouteEntries autorouteEntries,
            ILocalizationEntries localizationEntries,
            ISiteService siteService)
        {
            _autorouteEntries = autorouteEntries;
            _localizationEntries = localizationEntries;
            _siteService = siteService;
        }

        public async Task<string> GetContentItemIdFromRouteAsync(PathString url)
        {
            if (!url.HasValue)
            {
                url = "/";
            }

            string contentItemId = null;

            if (url == "/")
            {
                // get contentItemId from homeroute
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                contentItemId = siteSettings.HomeRoute["contentItemId"]?.ToString();
            }
            else
            {
                // Try to get from autorouteEntries.
                // This should not consider contained items, so will redirect to the parent item.
                (var found, var entry) = await _autorouteEntries.TryGetEntryByPathAsync(url.Value);

                if (found)
                {
                    contentItemId = entry.ContentItemId;
                };
            }

            if (string.IsNullOrEmpty(contentItemId))
            {
                return null;
            }

            return contentItemId;
        }

        public async Task<LocalizationEntry> GetLocalizationFromRouteAsync(PathString url)
        {
            var contentItemId = await GetContentItemIdFromRouteAsync(url);

            if (!string.IsNullOrEmpty(contentItemId))
            {
                (var found, var localization) = await _localizationEntries.TryGetLocalizationAsync(contentItemId);

                if (found)
                {
                    return localization;
                }
            }

            return null;
        }

        public async Task<IEnumerable<LocalizationEntry>> GetLocalizationsFromRouteAsync(PathString url)
        {
            var contentItemId = await GetContentItemIdFromRouteAsync(url);

            if (!string.IsNullOrEmpty(contentItemId))
            {
                (var found, var localization) = await _localizationEntries.TryGetLocalizationAsync(contentItemId);

                if (found)
                {
                    return await _localizationEntries.GetLocalizationsAsync(localization.LocalizationSet);
                }
            }

            return Enumerable.Empty<LocalizationEntry>();
        }
    }
}
