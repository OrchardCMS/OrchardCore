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
        private readonly IContentRoutingCoordinator _contentRoutingCoordinator;
        private readonly ILocalizationEntries _localizationEntries;
        private readonly ISiteService _siteService;


        public ContentCulturePickerService(
            IContentRoutingCoordinator contentRoutingCoordinator,
            ILocalizationEntries localizationEntries,
            ISiteService siteService)
        {
            _contentRoutingCoordinator = contentRoutingCoordinator;
            _localizationEntries = localizationEntries;
            _siteService = siteService;
        }

        public async Task<string> GetContentItemIdFromRouteAsync(PathString url)
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
                // try to get from content routing coordinator.
                // This will return the parent content item. When used for localization switching
                // this means a contained item will switch back to the container item, in the new culture.
                // We could look at switching to the contained item, but it would be based on order in the BagPart list
                // and would be easy to switch to an item that is not well related.
                _contentRoutingCoordinator.TryGetContentItemId(url.Value, out contentItemId);
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
                if (_localizationEntries.TryGetLocalization(contentItemId, out var localization))
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
                if (_localizationEntries.TryGetLocalization(contentItemId, out var localization))
                {
                    return _localizationEntries.GetLocalizations(localization.LocalizationSet);
                }
            }

            return Enumerable.Empty<LocalizationEntry>();
        }
    }
}
