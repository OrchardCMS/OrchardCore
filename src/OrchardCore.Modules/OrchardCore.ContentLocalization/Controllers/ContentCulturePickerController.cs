using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Autoroute.Services;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization;
using OrchardCore.Modules;

namespace OrchardCore.ContentLocalization.Controllers
{
    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
    public class ContentCulturePickerController : Controller, IUpdateModel
    {
        private readonly ILocalizationService _locationService;
        private readonly IContentCulturePickerService _culturePickerService;
        private readonly IAutorouteEntries _autorouteEntries;

        public IHtmlLocalizer T { get; }

        public ContentCulturePickerController(
            IHtmlLocalizer<AdminController> localizer,
            ILocalizationService locationService,
            IContentCulturePickerService culturePickerService,
            IAutorouteEntries autorouteEntries,
            ShellSettings shellSettings
           )
        {
            T = localizer;
            _locationService = locationService;
            _culturePickerService = culturePickerService;
            _autorouteEntries = autorouteEntries;
        }

        [HttpGet]
        public async Task<IActionResult> RedirectToLocalizedContent(string targetCulture, PathString contentItemUrl)
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
                return LocalRedirect('~' + contentItemUrl);
            }

            // Redirect the user to the Content with the same localizationSet as the ContentItem of the current url
            var contentItemId = await _culturePickerService.GetContentItemIdFromRoute(contentItemUrl);

            if (!string.IsNullOrEmpty(contentItemId))
            {
                var contentItem = await _culturePickerService.GetRelatedContentItem(contentItemId, targetCulture);

                if (contentItem != null && _autorouteEntries.TryGetPath(contentItem.ContentItemId, out var path))
                {
                    return LocalRedirect("~" + path);
                }
            }

            return LocalRedirect('~' + contentItemUrl);
        }
    }
}
