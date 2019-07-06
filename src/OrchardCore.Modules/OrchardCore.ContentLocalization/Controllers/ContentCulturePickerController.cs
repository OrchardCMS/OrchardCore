using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Localization;
using OrchardCore.Modules;

namespace OrchardCore.ContentLocalization.Controllers
{
    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
    public class ContentCulturePickerController : Controller, IUpdateModel
    {
        private readonly ILocalizationService _locationService;
        private readonly IContentCulturePickerService _culturePickerService;

        public IHtmlLocalizer T { get; }

        public ContentCulturePickerController(
            IHtmlLocalizer<AdminController> localizer,
            ILocalizationService locationService,
            IContentCulturePickerService culturePickerService)
        {
            T = localizer;
            _locationService = locationService;
            _culturePickerService = culturePickerService;
        }

        [HttpGet]
        public async Task<IActionResult> RedirectToLocalizedContent(string targetCulture, PathString contentItemUrl)
        {
            // Invariant culture name is empty so a null value is bound.
            targetCulture = targetCulture ?? "";

            if (!contentItemUrl.HasValue)
            {
                contentItemUrl = "/";
            }

            var supportedCultures = await _locationService.GetSupportedCulturesAsync();

            if (!supportedCultures.Any(t => t == targetCulture))
            {
                return LocalRedirect('~' + contentItemUrl);
            }

            // Redirect the user to the Content with the same localizationSet as the ContentItem of the current url
            var localizations = await _culturePickerService.GetLocalizationsFromRouteAsync(contentItemUrl);
            if (localizations.Any())
            {
                var localization = localizations.SingleOrDefault(l => String.Equals(l.Culture, targetCulture, StringComparison.OrdinalIgnoreCase));

                if (localization != null)
                {
                    return LocalRedirect(Url.Action("Display", "Item", new { Area = "OrchardCore.Contents", contentItemId = localization.ContentItemId }));
                }
            }

            // Try to get the Homepage url for the culture
            var homeLocalizations = await _culturePickerService.GetLocalizationsFromRouteAsync("/");
            if (homeLocalizations.Any())
            {
                var localization = homeLocalizations.SingleOrDefault(h => String.Equals(h.Culture, targetCulture, StringComparison.OrdinalIgnoreCase));

                if (localization != null)
                {
                    return LocalRedirect(Url.Action("Display", "Item", new { Area = "OrchardCore.Contents", contentItemId = localization.ContentItemId }));
                }
            }

            return NotFound();
        }
    }
}
