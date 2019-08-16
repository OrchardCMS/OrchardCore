using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Controllers
{
    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
    public class ContentCulturePickerController : Controller, IUpdateModel
    {
        private readonly ISiteService _siteService;
        private readonly ILocalizationService _locationService;
        private readonly IContentCulturePickerService _culturePickerService;

        public IHtmlLocalizer T { get; }

        public ContentCulturePickerController(
             ISiteService siteService,
            IHtmlLocalizer<AdminController> localizer,
            ILocalizationService locationService,
            IContentCulturePickerService culturePickerService)
        {
            _siteService = siteService;
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
            var settings = (await _siteService.GetSiteSettingsAsync()).As<ContentCulturePickerSettings>();

            if (settings.SetCookie)
            {
                // Set the cookie to handle redirecting to a custom controller
                Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(targetCulture)),
                    new CookieOptions { Expires = DateTime.UtcNow.AddDays(14) }
                );
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

            if (settings.RedirectToHomepage)
            {
                // Try to get the Homepage for the current culture
                var homeLocalizations = await _culturePickerService.GetLocalizationsFromRouteAsync("/");
                if (homeLocalizations.Any())
                {
                    var localization = homeLocalizations.SingleOrDefault(h => String.Equals(h.Culture, targetCulture, StringComparison.OrdinalIgnoreCase));

                    if (localization != null)
                    {
                        return LocalRedirect(Url.Action("Display", "Item", new { Area = "OrchardCore.Contents", contentItemId = localization.ContentItemId }));
                    }
                }
            }

            // Redirect to the same page by default
            return LocalRedirect('~' + contentItemUrl);
        }
    }
}
