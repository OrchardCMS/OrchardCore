using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Autoroute.Services;
using OrchardCore.Autoroute.Model;
using OrchardCore.Environment.Shell;
using OrchardCore.ContentLocalization.Services;

namespace OrchardCore.ContentLocalization.Controllers
{
    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
    public class ContentCulturePickerController : Controller, IUpdateModel
    {
        private readonly ILocalizationService _locationService;
        private readonly IContentCulturePickerService _culturePickerService;
        private readonly string _tenantPrefix;

        public IHtmlLocalizer T { get; }

        public ContentCulturePickerController(
            IHtmlLocalizer<AdminController> localizer,
            ILocalizationService locationService,
            IContentCulturePickerService culturePickerService,
            ShellSettings shellSettings
           )
        {
            T = localizer;
            _locationService = locationService;
            _tenantPrefix = "/" + shellSettings.RequestUrlPrefix;
            _culturePickerService = culturePickerService;
        }

        [HttpGet]
        public async Task<IActionResult> RedirectToLocalizedContent(string targetCulture, PathString contentItemUrl, string queryString)
        {
            if (contentItemUrl == "/Error")
            {
                return NoContent();
            }
            if (!contentItemUrl.HasValue)
            {
                contentItemUrl = "/";
            }
            var supportedCultures = await _locationService.GetSupportedCulturesAsync();
            if (!supportedCultures.Any(t => t == targetCulture))
            {
                return LocalRedirect('~' + contentItemUrl + queryString);
            }

            // Redirect the user to the Content with the same localizationSet as the ContentItem of the current url
            var localizations = await _culturePickerService.GetLocalizationsFromRouteAsync(contentItemUrl);
            if (localizations is object && localizations.Any())
            {
                var targetContent = localizations.SingleOrDefault(l => string.Equals(l.Culture, targetCulture, StringComparison.OrdinalIgnoreCase));

                if(targetContent is object)
                {
                    return LocalRedirect(Url.Action("Display", "Item", new { Area = "OrchardCore.Contents", contentItemId = targetContent.ContentItemId }));
                }
            }
            // Try to get the Homepage url for the culture

            var homeLocalizations = await _culturePickerService.GetLocalizationsFromRouteAsync("/");

            if (homeLocalizations.Any())
            {
                var localization = homeLocalizations.SingleOrDefault(h => string.Equals(h.Culture, targetCulture, StringComparison.OrdinalIgnoreCase));
                if (localization is object)
                {
                    return LocalRedirect(Url.Action("Display", "Item", new { Area = "OrchardCore.Contents", contentItemId = localization.ContentItemId }));
                }
            }

            return NotFound();
        }
    }
}