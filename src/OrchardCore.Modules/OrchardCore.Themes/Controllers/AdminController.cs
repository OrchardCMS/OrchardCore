using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules.Manifest;
using OrchardCore.Security;
using OrchardCore.Themes.Models;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IAdminThemeService _adminThemeService;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer H;

        public AdminController(
            ISiteThemeService siteThemeService,
            IAdminThemeService adminThemeService,
            IHtmlLocalizer<AdminController> localizer,
            IShellFeaturesManager shellFeaturesManager,
            IAuthorizationService authorizationService,
            INotifier notifier)
        {
            _siteThemeService = siteThemeService;
            _adminThemeService = adminThemeService;
            _shellFeaturesManager = shellFeaturesManager;
            _authorizationService = authorizationService;
            _notifier = notifier;

            H = localizer;
        }

        public async Task<ActionResult> Index()
        {
            var installThemes = await _authorizationService.AuthorizeAsync(User, StandardPermissions.SiteOwner); // only site owners

            if (!installThemes)
            {
                return Forbid();
            }

            var currentSiteThemeExtensionInfo = await _siteThemeService.GetSiteThemeAsync();
            var currentAdminThemeExtensionInfo = await _adminThemeService.GetAdminThemeAsync();
            var currentAdminTheme = currentAdminThemeExtensionInfo != null ? new ThemeEntry(currentAdminThemeExtensionInfo) : default(ThemeEntry);
            var currentSiteTheme = currentSiteThemeExtensionInfo != null ? new ThemeEntry(currentSiteThemeExtensionInfo) : default(ThemeEntry);
            var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

            var themes = (await _shellFeaturesManager.GetAvailableExtensionsAsync())
                .OfType<IThemeExtensionInfo>()
                .Where(extensionDescriptor =>
                {
                    var tags = extensionDescriptor.Manifest.Tags.ToArray();
                    var isHidden = tags.Any(x => String.Equals(x, "hidden", StringComparison.OrdinalIgnoreCase));
                    if (isHidden)
                    {
                        return false;
                    }

                    return true;
                })
                .Select(extensionDescriptor =>
                {
                    var isAdmin = IsAdminTheme(extensionDescriptor.Manifest);
                    var themeId = isAdmin ? currentAdminTheme?.Extension.Id : currentSiteTheme?.Extension.Id;
                    var isCurrent = extensionDescriptor.Id == themeId;
                    var isEnabled = enabledFeatures.Any(x => x.Extension.Id == extensionDescriptor.Id);
                    var themeEntry = new ThemeEntry(extensionDescriptor)
                    {
                        Enabled = isEnabled,
                        CanUninstall = installThemes,
                        IsAdmin = isAdmin,
                        IsCurrent = isCurrent
                    };

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
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
            {
                return Forbid();
            }

            if (String.IsNullOrEmpty(id))
            {
                // Don't use any theme on the front-end
            }
            else
            {
                var feature = (await _shellFeaturesManager.GetAvailableFeaturesAsync())
                    .FirstOrDefault(f => f.Extension.IsTheme() && f.Id == id);

                if (feature == null)
                {
                    return NotFound();
                }
                else
                {
                    var isAdmin = IsAdminTheme(feature.Extension.Manifest);

                    if (isAdmin)
                    {
                        await _adminThemeService.SetAdminThemeAsync(id);
                    }
                    else
                    {
                        await _siteThemeService.SetSiteThemeAsync(id);
                    }

                    // Enable the feature lastly to avoid accessing a disposed IMemoryCache (due to the current shell being disposed after updating).
                    var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();
                    var isEnabled = enabledFeatures.Any(x => x.Extension.Id == feature.Id);

                    if (!isEnabled)
                    {
                        await _shellFeaturesManager.EnableFeaturesAsync(new[] { feature }, force: true);
                        _notifier.Success(H["{0} was enabled", feature.Name ?? feature.Id]);
                    }

                    _notifier.Success(H["{0} was set as the default {1} theme", feature.Name ?? feature.Id, isAdmin ? "Admin" : "Site"]);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<ActionResult> ResetSiteTheme()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
            {
                return Forbid();
            }

            await _siteThemeService.SetSiteThemeAsync("");

            _notifier.Success(H["The Site theme was reset."]);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<ActionResult> ResetAdminTheme()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
            {
                return Forbid();
            }

            await _adminThemeService.SetAdminThemeAsync("");

            _notifier.Success(H["The Admin theme was reset."]);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
            {
                return Forbid();
            }

            var feature = (await _shellFeaturesManager.GetAvailableFeaturesAsync())
                .FirstOrDefault(f => f.Extension.IsTheme() && f.Id == id);

            if (feature == null)
            {
                return NotFound();
            }

            await _shellFeaturesManager.DisableFeaturesAsync(new[] { feature }, force: true);

            _notifier.Success(H["{0} was disabled.", feature.Name ?? feature.Id]);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApplyTheme))
            {
                return Forbid();
            }

            var feature = (await _shellFeaturesManager.GetAvailableFeaturesAsync())
                .FirstOrDefault(f => f.Extension.IsTheme() && f.Id == id);

            if (feature == null)
            {
                return NotFound();
            }

            await _shellFeaturesManager.EnableFeaturesAsync(new[] { feature }, force: true);

            _notifier.Success(H["{0} was enabled.", feature.Name ?? feature.Id]);

            return RedirectToAction(nameof(Index));
        }

        private static bool IsAdminTheme(IManifestInfo manifest)
        {
            return manifest.Tags.Any(x => String.Equals(x, ManifestConstants.AdminTag, StringComparison.OrdinalIgnoreCase));
        }
    }
}
