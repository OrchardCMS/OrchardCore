using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Controllers
{
    public class AdminController : Controller
    {
        private readonly IShellHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ShellSettings _currentShellSettings;
        private readonly INotifier _notifier;

        public AdminController(
            IShellHost orchardHost,
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
            var settings = _shellSettingsManager.LoadSettings();

            var model = new AdminIndexViewModel
            {
                ShellSettingsEntries = settings.Select(s => new ShellSettingsEntry
                {
                    Name = s.Name,
                    ShellSettings = s,
                    IsDefaultTenant = string.Equals(s.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase)
                }).ToList()
            };

            return View(model);
        }


        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (!IsDefaultShell())
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

            if (!IsDefaultShell())
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model, true);
            }

            if (ModelState.IsValid)
            {
                var shellSettings = new ShellSettings
                {
                    Name = model.Name,
                    RequestUrlPrefix = model.RequestUrlPrefix?.Trim(),
                    RequestUrlHost = model.RequestUrlHost,
                    ConnectionString = model.ConnectionString,
                    TablePrefix = model.TablePrefix,
                    DatabaseProvider = model.DatabaseProvider,
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

            if (!IsDefaultShell())
            {
                return Unauthorized();
            }

            var shellSettings = _shellSettingsManager.LoadSettings().FirstOrDefault(s => string.Equals(s.Name, id, StringComparison.Ordinal));

            if (shellSettings == null)
            {
                return NotFound();
            }

            var model = new EditTenantViewModel
            {
                Name = shellSettings.Name,
                RequestUrlHost = shellSettings.RequestUrlHost,
                RequestUrlPrefix = shellSettings.RequestUrlPrefix,
            };

            // The user can change the 'preset' database information only if the 
            // tenant has not been initialized yet
            if (shellSettings.State == TenantState.Uninitialized)
            {
                model.DatabaseProvider = shellSettings.DatabaseProvider;
                model.TablePrefix = shellSettings.TablePrefix;
                model.ConnectionString = shellSettings.ConnectionString;
                model.CanSetDatabasePresets = true;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditTenantViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (!IsDefaultShell())
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model, false);
            }

            var shellSettings = _shellSettingsManager.LoadSettings().FirstOrDefault(s => string.Equals(s.Name, model.Name, StringComparison.Ordinal));

            if (shellSettings == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                shellSettings.RequestUrlPrefix = model.RequestUrlPrefix?.Trim();
                shellSettings.RequestUrlHost = model.RequestUrlHost;

                // The user can change the 'preset' database information only if the 
                // tenant has not been initialized yet
                if (shellSettings.State == TenantState.Uninitialized)
                {
                    shellSettings.DatabaseProvider = model.DatabaseProvider;
                    shellSettings.TablePrefix = model.TablePrefix;
                    shellSettings.ConnectionString = model.ConnectionString;
                }

                _orchardHost.UpdateShellSettings(shellSettings);

                return RedirectToAction(nameof(Index));
            }

            // The user can change the 'preset' database information only if the 
            // tenant has not been initialized yet
            if (shellSettings.State == TenantState.Uninitialized)
            {
                model.DatabaseProvider = shellSettings.DatabaseProvider;
                model.TablePrefix = shellSettings.TablePrefix;
                model.ConnectionString = shellSettings.ConnectionString;
                model.CanSetDatabasePresets = true;
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

            if (!IsDefaultShell())
            {
                return Unauthorized();
            }

            var shellSettings = _shellSettingsManager.LoadSettings().FirstOrDefault(s => string.Equals(s.Name, id, StringComparison.Ordinal));

            if (shellSettings == null)
            {
                return NotFound();
            }

            if (string.Equals(shellSettings.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase))
            {
                _notifier.Error(H["You cannot disable the default tenant."]);
                return RedirectToAction(nameof(Index));
            }

            if (shellSettings.State != TenantState.Running)
            {
                _notifier.Error(H["You can only disable a Running shell."]);
                return RedirectToAction(nameof(Index));
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

            if (!IsDefaultShell())
            {
                return Unauthorized();
            }

            var shellSettings = _shellSettingsManager.LoadSettings().FirstOrDefault(s => string.Equals(s.Name, id, StringComparison.Ordinal));

            if (shellSettings == null)
            {
                return NotFound();
            }

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

            if (!IsDefaultShell())
            {
                return Unauthorized();
            }

            var shellSettings = _shellSettingsManager.LoadSettings().FirstOrDefault(s => string.Equals(s.Name, id, StringComparison.Ordinal));

            if (shellSettings == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var redirectUrl = Url.Action(nameof(Index));

            _orchardHost.ReloadShellContext(shellSettings);

            return Redirect(redirectUrl);
        }

        private void ValidateViewModel(EditTenantViewModel model, bool newTenant)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["The tenant name is mandatory."]);
            }

            var allSettings = _shellSettingsManager.LoadSettings();

            if (newTenant && allSettings.Any(s => string.Equals(s.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["A tenant with the same name already exists.", model.Name]);
            }

            if (!string.IsNullOrEmpty(model.Name) && !Regex.IsMatch(model.Name, @"^\w+$"))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["Invalid tenant name. Must contain characters only and no spaces."]);
            }

            if (!IsDefaultShell() && string.IsNullOrWhiteSpace(model.RequestUrlHost) && string.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["Host and url prefix can not be empty at the same time."]);
            }

            var allOtherSettings = allSettings.Where(s => !string.Equals(s.Name, model.Name, StringComparison.OrdinalIgnoreCase));
            if (allOtherSettings.Any(s => string.Equals(s.RequestUrlPrefix, model.RequestUrlPrefix?.Trim(), StringComparison.OrdinalIgnoreCase) && string.Equals(s.RequestUrlHost, model.RequestUrlHost, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["A tenant with the same host and prefix already exists.", model.Name]);
            }

            if (!string.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                if (model.RequestUrlPrefix.Contains('/'))
                {
                    ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["The url prefix can not contains more than one segment."]);
                }
            }
        }

        private bool IsDefaultShell()
        {
            return string.Equals(_currentShellSettings.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
