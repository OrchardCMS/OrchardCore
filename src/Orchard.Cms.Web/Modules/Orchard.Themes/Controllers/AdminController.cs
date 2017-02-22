using System;
using System.Linq;
using System.Threading.Tasks;
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
            var installThemes = await _authorizationService.AuthorizeAsync(User, StandardPermissions.SiteOwner); // only site owners
            //&& _shellSettings.Name == ShellSettings.; // of the default tenant
            //&& _featureManager.GetEnabledFeatures().FirstOrDefault(f => f.Id == "PackagingServices") != null

            //var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();

            var currentSiteThemeExtensionInfo = await _siteThemeService.GetSiteThemeAsync();
            var currentAdminThemeExtensionInfo = await _adminThemeService.GetAdminThemeAsync();
            var currentAdminTheme = currentAdminThemeExtensionInfo != null ? new ThemeEntry(currentAdminThemeExtensionInfo) : default(ThemeEntry);
            var currentSiteTheme = currentSiteThemeExtensionInfo != null ? new ThemeEntry(currentSiteThemeExtensionInfo) : default(ThemeEntry);
            var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

            // TODO: Cast the IExtensionInfo objects to ThemeExtensionInfo objects once Nick fixes the issue where both Modules and Themes are constructed as ExtensionInfos.
            // var themes = _extensionManager.GetExtensions().OfType<ThemeExtensionInfo>().Where(extensionDescriptor =>
            var themes = _extensionManager.GetExtensions().Where(extensionDescriptor =>
            {
                var isTheme = extensionDescriptor.Manifest.IsTheme();
                var tags = extensionDescriptor.Manifest.Tags.ToArray();
                var isHidden = tags.Any(x => string.Equals(x, "hidden", StringComparison.OrdinalIgnoreCase));
                
                /// is the theme allowed for this tenant ?
                // allowed = _shellSettings.Themes.Length == 0 || _shellSettings.Themes.Contains(extensionDescriptor.Id);

                return isTheme && !isHidden;
            })
            .Select(extensionDescriptor =>
            {
                var isAdmin = IsAdminTheme(extensionDescriptor.Manifest);
                var themeId = isAdmin ? currentAdminTheme?.Extension.Id : currentSiteTheme?.Extension.Id;
                var isCurrent = extensionDescriptor.Id == themeId;
                var isEnabled = enabledFeatures.Any(x => x.Extension.Id == extensionDescriptor.Id);
                var themeEntry = new ThemeEntry(extensionDescriptor)
                {
                    //NeedsUpdate = featuresThatNeedUpdate.Contains(extensionDescriptor.Id),
                    //IsRecentlyInstalled = _themeService.IsRecentlyInstalled(extensionDescriptor),
                    Enabled = isEnabled,
                    CanUninstall = installThemes,
                    IsAdmin = isAdmin,
                    IsCurrent = isCurrent
                };

                //if (_extensionDisplayEventHandler != null)
                //{
                //    foreach (string notification in _extensionDisplayEventHandler.Displaying(themeEntry.Descriptor, ControllerContext.RequestContext))
                //    {
                //        themeEntry.Notifications.Add(notification);
                //    }
                //}

                return themeEntry;
            })
            .OrderByDescending(x => x.IsCurrent);

            var model = new SelectThemesViewModel
            {
                CurrentSiteTheme = currentSiteTheme,
                CurrentAdminTheme = currentAdminTheme,
                Themes = themes
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SetCurrentTheme(string id)
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
                var isAdmin = IsAdminTheme(feature.Extension.Manifest);

                if (isAdmin)
                    await _adminThemeService.SetAdminThemeAsync(id);
                else
                    await _siteThemeService.SetSiteThemeAsync(id);

                // Enable the feature lastly to avoid accessing a disposed IMemoryCache (due to the current shell being disposed after updating).
                var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();
                var isEnabled = enabledFeatures.Any(x => x.Extension.Id == feature.Id);

                if (!isEnabled)
                {
                    await _shellFeaturesManager.EnableFeaturesAsync(new[] { feature }, force: true);
                    _notifier.Success(T["{0} was enabled", feature.Name ?? feature.Id]);
                }

                _notifier.Success(T["{0} was set as the default {1} theme", feature.Name ?? feature.Id, isAdmin ? "Admin" : "Site"]);
            }

            return RedirectToAction("Index");
        }
        
        [HttpPost]
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

        [HttpPost]
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

        private bool IsAdminTheme(IManifestInfo manifest)
        {
            return manifest.Tags.Any(x => string.Equals(x, "admin", StringComparison.OrdinalIgnoreCase));
        }
    }
}