using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement.Notify;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Models;
using Orchard.Hosting;
using Orchard.Tenants.ViewModels;

namespace Orchard.Tenants.Controllers
{
    public class AdminController : Controller
    {
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ShellSettings _currentShellSettings;
        private readonly INotifier _notifier;

        public AdminController(
            IOrchardHost orchardHost, 
            ShellSettings currentShellSettings,
            IAuthorizationService authorizationService,
            IShellSettingsManager shellSettingsManager,
            INotifier notifier,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer)
        {
            _orchardHost = orchardHost;
            _authorizationService = authorizationService;
            _shellSettingsManager = shellSettingsManager;
            _currentShellSettings = currentShellSettings;
            _notifier = notifier;

            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public IStringLocalizer S { get; set; }
        public IHtmlLocalizer H { get; set; }

        public IActionResult Index()
        {
            var shells = _orchardHost.ListShellContexts();

            var model = new AdminIndexViewModel
            {
                ShellSettingsEntries = shells.Select(x => new ShellSettingsEntry { Name = x.Settings.Name, ShellSettings = x.Settings }).ToList()
            };

            return View(model);
        }


        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (_currentShellSettings.Name != ShellHelper.DefaultShellName)
            {
                return Unauthorized();
            }

            var model = new EditTenantViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EditTenantViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (_currentShellSettings.Name != ShellHelper.DefaultShellName)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model);
            }

            if (ModelState.IsValid)
            {
                var shellSettings = new ShellSettings
                {
                    Name = model.Name,
                    RequestUrlPrefix = model.RequestUrlPrefix,
                    RequestUrlHost = model.RequestUrlHost,
                    State = TenantState.Uninitialized
                };

                _shellSettingsManager.SaveSettings(shellSettings);
                var shellContext = _orchardHost.GetOrCreateShellContext(shellSettings);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (_currentShellSettings.Name != ShellHelper.DefaultShellName)
            {
                return Unauthorized();
            }

            var shellContext = _orchardHost
                .ListShellContexts()
                .Where(x => String.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (shellContext == null)
            {
                return NotFound();
            }

            var shellSettings = shellContext.Settings;

            var model = new EditTenantViewModel
            {
                Name = shellSettings.Name,
                RequestUrlHost = shellSettings.RequestUrlHost,
                RequestUrlPrefix = shellSettings.RequestUrlPrefix
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditTenantViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (_currentShellSettings.Name != ShellHelper.DefaultShellName)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model);
            }

            if (ModelState.IsValid)
            {
                var shellContext = _orchardHost
                .ListShellContexts()
                .Where(x => String.Equals(x.Settings.Name, model.Name, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

                if (shellContext == null)
                {
                    return NotFound();
                }

                var shellSettings = shellContext.Settings;

                if (shellSettings.RequestUrlHost != model.RequestUrlHost || shellSettings.RequestUrlPrefix != model.RequestUrlPrefix)
                {
                    shellSettings.RequestUrlPrefix = model.RequestUrlPrefix;
                    shellSettings.RequestUrlHost = model.RequestUrlHost;

                    _orchardHost.UpdateShellSettings(shellSettings);
                }

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (_currentShellSettings.Name != ShellHelper.DefaultShellName)
            {
                return Unauthorized();
            }

            var shellContext = _orchardHost
                .ListShellContexts()
                .Where(x => String.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (shellContext == null)
            {
                return NotFound();
            }

            var shellSettings = shellContext.Settings;

            if (shellSettings.State != TenantState.Running)
            {
                _notifier.Error(H["You can only disable a Running shell."]);
            }

            shellSettings.State = TenantState.Disabled;
            _orchardHost.UpdateShellSettings(shellSettings);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (_currentShellSettings.Name != ShellHelper.DefaultShellName)
            {
                return Unauthorized();
            }

            var shellContext = _orchardHost
                .ListShellContexts()
                .Where(x => String.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (shellContext == null)
            {
                return NotFound();
            }

            var shellSettings = shellContext.Settings;

            if (shellSettings.State != TenantState.Disabled)
            {
                _notifier.Error(H["You can only enable a Disabled shell."]);
            }

            shellSettings.State = TenantState.Running;
            _orchardHost.UpdateShellSettings(shellSettings);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reload(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (_currentShellSettings.Name != ShellHelper.DefaultShellName)
            {
                return Unauthorized();
            }

            var shellContext = _orchardHost
                .ListShellContexts()
                .Where(x => String.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (shellContext == null)
            {
                return NotFound();
            }

            var shellSettings = shellContext.Settings;
            _orchardHost.ReloadShellContext(shellSettings);

            return RedirectToAction(nameof(Index));
        }
        private void ValidateViewModel(EditTenantViewModel model)
        {
            if (String.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["The tenant name is mandatory."]);
            }

            var allShells = _orchardHost.ListShellContexts();

            if (allShells.Any(tenant => String.Equals(tenant.Settings.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["A tenant with the same name already exists.", model.Name]);
            }

            if (!String.IsNullOrEmpty(model.Name) && !Regex.IsMatch(model.Name, @"^\w+$"))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["Invalid tenant name. Must contain characters only and no spaces."]);
            }

            if (!String.Equals(model.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(model.RequestUrlHost) && string.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["Host and Url Prefix can not be empty at the same time."]);
            }
        }
    }
}
