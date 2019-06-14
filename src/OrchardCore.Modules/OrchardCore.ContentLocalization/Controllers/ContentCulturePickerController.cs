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
        public async Task<IActionResult> RedirectToLocalizedContent(string targetCulture, string contentItemUrl)
        {
            if (string.IsNullOrEmpty(contentItemUrl))
            {
                return NotFound();
            }

            var supportedCultures = await _locationService.GetSupportedCulturesAsync();
            if (!supportedCultures.Any(t => t == targetCulture))
            {
                return LocalRedirect(contentItemUrl);
            }

            // Redirect the user to the Content with the same localizationSet as the ContentItem of the current url
            var contentItemId = await _culturePickerService.GetContentItemIdFromRoute(contentItemUrl);
            if (!string.IsNullOrEmpty(contentItemId))
            {
                var contentItem = await _culturePickerService.GetRelatedContentItem(contentItemId, targetCulture);
                var path = contentItem.As<AutoroutePart>()?.Path;
                if (!string.IsNullOrEmpty(path))
                {
                    // Temporarily set the cookie here for testing. Setting the culture should be the job of the ContentRequestCultureProvider
                    Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
                    Response.Cookies.Append(
                        CookieRequestCultureProvider.DefaultCookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(targetCulture)),
                        new CookieOptions { Expires = DateTime.UtcNow.AddMonths(1) }
                    );
                    return LocalRedirect(string.Join("/", _tenantPrefix, path));
                }
            }
            return LocalRedirect(contentItemUrl);
        }
    }
}
