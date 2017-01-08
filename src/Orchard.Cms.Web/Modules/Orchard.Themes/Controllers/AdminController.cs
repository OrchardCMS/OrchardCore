using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Orchard.Admin;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Notify;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Security;
using Orchard.Themes.Models;
using Orchard.Themes.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Themes.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IAdminThemeService _adminThemeService;
        private readonly IThemeService _themeService;
        private readonly ShellSettings _shellSettings;
        private readonly IExtensionManager _extensionManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;

        public AdminController(
            ISiteThemeService siteThemeService,
            IAdminThemeService adminThemeService,
            IThemeService themeService,
            ShellSettings shellSettings,
            IExtensionManager extensionManager,
            IHtmlLocalizer<AdminController> localizer,
            IShellDescriptorManager shellDescriptorManager,
            IShellFeaturesManager shellFeaturesManager,
            IAuthorizationService authorizationService,
            INotifier notifier)
        {
            _siteThemeService = siteThemeService;
            _adminThemeService = adminThemeService;
            _themeService = themeService;
            _shellSettings = shellSettings;
            _extensionManager = extensionManager;
            _shellDescriptorManager = shellDescriptorManager;
            _shellFeaturesManager = shellFeaturesManager;
            _authorizationService = authorizationService;
            _notifier = notifier;

            T = localizer;
        }

        public IHtmlLocalizer T { get; }

        public async Task<ActionResult> Index()
        {
            bool installThemes = await _authorizationService.AuthorizeAsync(User, StandardPermissions.SiteOwner); // only site owners
            //&& _shellSettings.Name == ShellSettings.; // of the default tenant
            //&& _featureManager.GetEnabledFeatures().FirstOrDefault(f => f.Id == "PackagingServices") != null

            //var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();

            ThemeEntry currentSiteTheme = null;
            IExtensionInfo currentSiteThemeExtensionInfo = await _siteThemeService.GetSiteThemeAsync();
            if (currentSiteThemeExtensionInfo != null)
            {
                currentSiteTheme = new ThemeEntry(currentSiteThemeExtensionInfo);
            }

            ThemeEntry currentAdminTheme = null;
            IExtensionInfo currentAdminThemeExtensionInfo = await _adminThemeService.GetAdminThemeAsync();
            if (currentAdminThemeExtensionInfo != null)
            {
                currentAdminTheme = new ThemeEntry(currentAdminThemeExtensionInfo);
            }
            
            var features = _extensionManager.GetFeatures();
            var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

            IEnumerable<ThemeEntry> themes = features
                .Where(extensionDescriptor =>
                {
                    //bool hidden = false;
                    //string tags = extensionDescriptor.Tags;
                    //if (tags != null)
                    //{
                    //    hidden = tags.Split(',').Any(t => t.Trim().Equals("hidden", StringComparison.OrdinalIgnoreCase));
                    //}

                    //// is the theme allowed for this tenant ?
                    // allowed = _shellSettings.Themes.Length == 0 || _shellSettings.Themes.Contains(extensionDescriptor.Id);

                    //!hidden && allowed &&
                    return
                            extensionDescriptor.Extension.Manifest.IsTheme() &&
                            (currentSiteTheme == null || !currentSiteTheme.Extension.Id.Equals(extensionDescriptor.Id)) &&
                            (currentAdminTheme == null || !currentAdminTheme.Extension.Id.Equals(extensionDescriptor.Id));
                })
                .Select(extensionDescriptor =>
                {
                    ThemeEntry themeEntry = new ThemeEntry(extensionDescriptor.Extension)
                    {
                        //NeedsUpdate = featuresThatNeedUpdate.Contains(extensionDescriptor.Id),
                        //IsRecentlyInstalled = _themeService.IsRecentlyInstalled(extensionDescriptor),
                        Enabled = enabledFeatures.Any(sf => sf.Name == extensionDescriptor.Id),
                        CanUninstall = installThemes
                    };

                    //if (_extensionDisplayEventHandler != null)
                    //{
                    //    foreach (string notification in _extensionDisplayEventHandler.Displaying(themeEntry.Descriptor, ControllerContext.RequestContext))
                    //    {
                    //        themeEntry.Notifications.Add(notification);
                    //    }
                    //}

                    return themeEntry;
                });

            var model = new SelectThemesViewModel
            {
                CurrentSiteTheme = currentSiteTheme,
                CurrentAdminTheme = currentAdminTheme,
                Themes = themes
            };

            return View(model);
        }

        //[HttpPost]
        public async Task<ActionResult> SetCurrentSiteTheme(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme)) // , T["Couldn't set the current theme."]
            {
                return Unauthorized();
            }

            var feature = _extensionManager.GetFeatures().FirstOrDefault(f => f.Extension.Manifest.IsTheme() && f.Id == id);

            if (feature == null)
            {
                return NotFound();
            }
            else
            {
                //await _shellFeaturesManager.EnableFeaturesAsync(new[] { feature }, force: true); // TODO : If the thme is Enabled then Set, there is a memory cache error.
                await _siteThemeService.SetSiteThemeAsync(id);
            }

            return RedirectToAction("Index");
        }

        //[HttpPost]
        public async Task<ActionResult> SetCurrentAdminTheme(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme)) // , T["Couldn't set the current admin theme."]
            {
                return Unauthorized();
            }

            var feature = _extensionManager.GetFeatures().FirstOrDefault(f => f.Extension.Manifest.IsTheme() && f.Id == id);

            if (feature == null)
            {
                return NotFound();
            }
            else
            {
                await _shellFeaturesManager.EnableFeaturesAsync(new[] { feature }, force: true);
                await _adminThemeService.SetAdminThemeAsync(id);
            }

            return RedirectToAction("Index");
        }

        //[HttpPost]
        public async Task<IActionResult> Disable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme)) // , T["Not allowed to apply theme."]
            {
                return Unauthorized();
            }

            var feature = _extensionManager.GetFeatures().FirstOrDefault(f => f.Extension.Manifest.IsTheme() && f.Id == id);

            if (feature == null)
            {
                return NotFound();
            }

            await _shellFeaturesManager.DisableFeaturesAsync(new[] { feature }, force: true);

            _notifier.Success(T["{0} was disabled", feature.Name ?? feature.Id]);

            return RedirectToAction("Index");
        }

        //[HttpPost]
        public async Task<IActionResult> Enable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme)) // , T["Not allowed to apply theme."]
            {
                return Unauthorized();
            }

            var feature = _extensionManager.GetFeatures().FirstOrDefault(f => f.Extension.Manifest.IsTheme() && f.Id == id);

            if (feature == null)
            {
                return NotFound();
            }

            await _shellFeaturesManager.EnableFeaturesAsync(new[] { feature }, force: true);

            _notifier.Success(T["{0} was enabled", feature.Name ?? feature.Id]);

            return RedirectToAction("Index");
        }
    }
}