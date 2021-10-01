using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.Entities;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Controllers
{
    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
    public class ContentCulturePickerController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly ILocalizationService _locationService;
        private readonly IContentCulturePickerService _culturePickerService;

        public ContentCulturePickerController(
            ISiteService siteService,
            ILocalizationService locationService,
            IContentCulturePickerService culturePickerService)
        {
            _siteService = siteService;
            _locationService = locationService;
            _culturePickerService = culturePickerService;
        }

        [HttpGet]
        public async Task<IActionResult> RedirectToLocalizedContent(string targetCulture, PathString contentItemUrl, string queryStringValue)
        {
            targetCulture ??= CultureInfo.InvariantCulture.Name;

            if (!contentItemUrl.HasValue)
            {
                contentItemUrl = "/";
            }

            var queryString = new QueryString(queryStringValue);
            var url = HttpContext.Request.PathBase + contentItemUrl + queryString;

            var supportedCultures = await _locationService.GetSupportedCulturesAsync();

            if (supportedCultures.Any(t => t == targetCulture))
            {
                var settings = (await _siteService.GetSiteSettingsAsync()).As<ContentCulturePickerSettings>();
                if (settings.SetCookie)
                {
                    _culturePickerService.SetContentCulturePickerCookie(targetCulture);
                }

                // Redirect the user to the Content with the same localizationSet as the ContentItem of the current url
                var localizations = await _culturePickerService.GetLocalizationsFromRouteAsync(contentItemUrl);
                if (!TryGetLocalizedContentUrl(localizations) && settings.RedirectToHomepage)
                {
                    // Try to get the Homepage for the current culture
                    var homeLocalizations = await _culturePickerService.GetLocalizationsFromRouteAsync("/");
                    TryGetLocalizedContentUrl(homeLocalizations);
                }
            }

            bool TryGetLocalizedContentUrl(IEnumerable<LocalizationEntry> localizationEntries)
            {
                if (localizationEntries.Any())
                {
                    var localization = localizationEntries.SingleOrDefault(e => String.Equals(e.Culture, targetCulture, StringComparison.OrdinalIgnoreCase));

                    if (localization != null)
                    {
                        url = Url.Action("Display", "Item", new { Area = "OrchardCore.Contents", contentItemId = localization.ContentItemId }) + queryString;

                        return true;
                    }
                }

                return false;
            }

            return LocalRedirect(url);
        }
    }
}
