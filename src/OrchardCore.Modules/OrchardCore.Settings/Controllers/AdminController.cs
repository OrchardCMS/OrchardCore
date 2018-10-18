using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IDisplayManager<ISite> _siteSettingsDisplayManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;

        public AdminController(
            ISiteService siteService,
            IDisplayManager<ISite> siteSettingsDisplayManager,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IHtmlLocalizer<AdminController> h,
            IStringLocalizer<AdminController> s
            )
        {
            _siteSettingsDisplayManager = siteSettingsDisplayManager;
            _siteService = siteService;
            _notifier = notifier;
            _authorizationService = authorizationService;
            H = h;
            S = s;
        }

        IHtmlLocalizer H { get; set; }
        IStringLocalizer S { get; set; }

        public async Task<IActionResult> Index(string groupId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageGroupSettings, (object)groupId))
            {
                return Unauthorized();
            }

            var site = await _siteService.GetSiteSettingsAsync();

            var viewModel = new AdminIndexViewModel
            {
                GroupId = groupId,
                Shape = await _siteSettingsDisplayManager.BuildEditorAsync(site, this, false, groupId)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost(string groupId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageGroupSettings, (object)groupId))
            {
                return Unauthorized();
            }

            var cachedSite = await _siteService.GetSiteSettingsAsync();

            // Clone the settings as the driver will update it and as it's a globally cached object
            // it would stay this way even on validation errors.

            var site = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(cachedSite, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }), cachedSite.GetType()) as ISite;

            var viewModel = new AdminIndexViewModel
            {
                GroupId = groupId,
                Shape = await _siteSettingsDisplayManager.UpdateEditorAsync(site, this, false, groupId)
            };

            if (ModelState.IsValid)
            {
                await _siteService.UpdateSiteSettingsAsync(site);

                _notifier.Success(H["Site settings updated successfully."]);

                return RedirectToAction(nameof(Index), new { groupId });
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Culture()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSettings))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            var model = new SiteCulturesViewModel
            {
                CurrentCulture = siteSettings.Culture,
                SiteCultures = siteSettings.SupportedCultures
            };

            model.AvailableSystemCultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => c.Name != String.Empty)
                .Select(ci => CultureInfo.GetCultureInfo(ci.Name))
                .Where(s => !model.SiteCultures.Contains(s.Name));

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddCulture(string systemCultureName, string cultureName)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSettings))
            {
                return Unauthorized();
            }

            cultureName = String.IsNullOrWhiteSpace(cultureName) ? systemCultureName : cultureName;

            if (!String.IsNullOrWhiteSpace(cultureName) && Regex.IsMatch(cultureName, "^[a-zA-Z]{1,8}(?:-[a-zA-Z0-9]{1,8})*$"))
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                siteSettings.SupportedCultures = siteSettings.SupportedCultures.Union(new[] { cultureName }, StringComparer.OrdinalIgnoreCase).ToArray();
                await _siteService.UpdateSiteSettingsAsync(siteSettings);

                _notifier.Warning(H["The site needs to be restarted for the settings to take effect"]);
            }
            else
            {
                ModelState.AddModelError(nameof(cultureName), S["Invalid culture name"]);
            }

            return RedirectToAction("Culture");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCulture(string cultureName)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSettings))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            siteSettings.SupportedCultures = siteSettings.SupportedCultures.Except(new[] { cultureName }, StringComparer.OrdinalIgnoreCase).ToArray();
            await _siteService.UpdateSiteSettingsAsync(siteSettings);

            _notifier.Warning(H["The site needs to be restarted for the settings to take effect"]);

            return RedirectToAction("Culture");
        }
    }
}