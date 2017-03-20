using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement.Notify;
using OrchardCore.Tenant;
using OrchardCore.Tenant.Models;
using Orchard.Hosting;
using Orchard.Hosting.TenantBuilders;
using Orchard.Tenants.ViewModels;

namespace Orchard.Tenants.Controllers
{
    public class AdminController : Controller
    {
        private readonly ITenantHost _orchardHost;
        private readonly ITenantSettingsManager _tenantSettingsManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly TenantSettings _currentTenantSettings;
        private readonly INotifier _notifier;

        public AdminController(
            ITenantHost orchardHost,
            TenantSettings currentTenantSettings,
            IAuthorizationService authorizationService,
            ITenantSettingsManager tenantSettingsManager,
            INotifier notifier,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer)
        {
            _orchardHost = orchardHost;
            _authorizationService = authorizationService;
            _tenantSettingsManager = tenantSettingsManager;
            _currentTenantSettings = currentTenantSettings;
            _notifier = notifier;

            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public IStringLocalizer S { get; set; }
        public IHtmlLocalizer H { get; set; }

        public IActionResult Index()
        {
            var tenants = GetTenants();

            var model = new AdminIndexViewModel
            {
                TenantSettingsEntries = tenants.Select(x => new TenantSettingsEntry
                {
                    Name = x.Settings.Name,
                    TenantSettings = x.Settings,
                    IsDefaultTenant = string.Equals(x.Settings.Name, TenantHelper.DefaultTenantName, StringComparison.OrdinalIgnoreCase)
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

            if (!IsDefaultTenant())
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

            if (!IsDefaultTenant())
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model, true);
            }

            if (ModelState.IsValid)
            {
                var tenantSettings = new TenantSettings
                {
                    Name = model.Name,
                    RequestUrlPrefix = model.RequestUrlPrefix,
                    RequestUrlHost = model.RequestUrlHost,
                    ConnectionString = model.ConnectionString,
                    TablePrefix = model.TablePrefix,
                    DatabaseProvider = model.DatabaseProvider,
                    State = TenantState.Uninitialized
                };

                _tenantSettingsManager.SaveSettings(tenantSettings);
                var tenantContext = _orchardHost.GetOrCreateTenantContext(tenantSettings);

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

            if (!IsDefaultTenant())
            {
                return Unauthorized();
            }

            var tenantContext = GetTenants()
                .Where(x => String.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (tenantContext == null)
            {
                return NotFound();
            }

            var tenantSettings = tenantContext.Settings;

            var model = new EditTenantViewModel
            {
                Name = tenantSettings.Name,
                RequestUrlHost = tenantSettings.RequestUrlHost,
                RequestUrlPrefix = tenantSettings.RequestUrlPrefix,
            };

            // The user can change the 'preset' database information only if the
            // tenant has not been initialized yet
            if (tenantSettings.State == TenantState.Uninitialized)
            {
                model.DatabaseProvider = tenantSettings.DatabaseProvider;
                model.TablePrefix = tenantSettings.TablePrefix;
                model.ConnectionString = tenantSettings.ConnectionString;
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

            if (!IsDefaultTenant())
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model, false);
            }

            var tenantContext = GetTenants()
                .Where(x => String.Equals(x.Settings.Name, model.Name, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (tenantContext == null)
            {
                return NotFound();
            }

            var tenantSettings = tenantContext.Settings;

            if (ModelState.IsValid)
            {
                tenantSettings.RequestUrlPrefix = model.RequestUrlPrefix;
                tenantSettings.RequestUrlHost = model.RequestUrlHost;

                // The user can change the 'preset' database information only if the
                // tenant has not been initialized yet
                if (tenantSettings.State == TenantState.Uninitialized)
                {
                    tenantSettings.DatabaseProvider = model.DatabaseProvider;
                    tenantSettings.TablePrefix = model.TablePrefix;
                    tenantSettings.ConnectionString = model.ConnectionString;
                }

                _orchardHost.UpdateTenantSettings(tenantSettings);

                return RedirectToAction(nameof(Index));
            }

            // The user can change the 'preset' database information only if the
            // tenant has not been initialized yet
            if (tenantSettings.State == TenantState.Uninitialized)
            {
                model.DatabaseProvider = tenantSettings.DatabaseProvider;
                model.TablePrefix = tenantSettings.TablePrefix;
                model.ConnectionString = tenantSettings.ConnectionString;
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

            if (!IsDefaultTenant())
            {
                return Unauthorized();
            }

            var tenantContext = GetTenants()
                .Where(x => String.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (tenantContext == null)
            {
                return NotFound();
            }

            var tenantSettings = tenantContext.Settings;

            if (string.Equals(tenantSettings.Name, TenantHelper.DefaultTenantName, StringComparison.OrdinalIgnoreCase))
            {
                _notifier.Error(H["You cannot disable the default tenant."]);
                return RedirectToAction(nameof(Index));
            }

            if (tenantSettings.State != TenantState.Running)
            {
                _notifier.Error(H["You can only disable a Running tenant."]);
                return RedirectToAction(nameof(Index));
            }

            tenantSettings.State = TenantState.Disabled;
            _orchardHost.UpdateTenantSettings(tenantSettings);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (!IsDefaultTenant())
            {
                return Unauthorized();
            }

            var tenantContext = _orchardHost
                .ListTenantContexts()
                .OrderBy(x => x.Settings.Name)
                .Where(x => String.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (tenantContext == null)
            {
                return NotFound();
            }

            var tenantSettings = tenantContext.Settings;

            if (tenantSettings.State != TenantState.Disabled)
            {
                _notifier.Error(H["You can only enable a Disabled tenant."]);
            }

            tenantSettings.State = TenantState.Running;
            _orchardHost.UpdateTenantSettings(tenantSettings);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reload(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (!IsDefaultTenant())
            {
                return Unauthorized();
            }

            var tenantContext = _orchardHost
                .ListTenantContexts()
                .OrderBy(x => x.Settings.Name)
                .Where(x => String.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (tenantContext == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var redirectUrl = Url.Action(nameof(Index));

            var tenantSettings = tenantContext.Settings;
            _orchardHost.ReloadTenantContext(tenantSettings);

            return Redirect(redirectUrl);
        }

        private void ValidateViewModel(EditTenantViewModel model, bool newTenant)
        {
            if (String.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["The tenant name is mandatory."]);
            }

            var allTenants = GetTenants();

            if (newTenant && allTenants.Any(tenant => String.Equals(tenant.Settings.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["A tenant with the same name already exists.", model.Name]);
            }

            if (newTenant && allTenants.Any(tenant => String.Equals(tenant.Settings.RequestUrlPrefix, model.RequestUrlPrefix, StringComparison.OrdinalIgnoreCase) && String.Equals(tenant.Settings.RequestUrlHost, model.RequestUrlHost, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["A tenant with the same host and prefix already exists.", model.Name]);
            }

            if (!String.IsNullOrEmpty(model.Name) && !Regex.IsMatch(model.Name, @"^\w+$"))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["Invalid tenant name. Must contain characters only and no spaces."]);
            }

            if (!IsDefaultTenant() && string.IsNullOrWhiteSpace(model.RequestUrlHost) && string.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["Host and url prefix can not be empty at the same time."]);
            }

            if (!String.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                if (model.RequestUrlPrefix.Contains('/'))
                {
                    ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["The url prefix can not contains more than one segment."]);
                }

                if (allTenants.Any(x => x.Settings.RequestUrlPrefix != null && String.Equals(x.Settings.RequestUrlPrefix.Trim(), model.RequestUrlPrefix.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["The url prefix is already used by another tenant."]);
                }
            }
        }

        private IEnumerable<TenantContext> GetTenants()
        {
            return _orchardHost.ListTenantContexts().OrderBy(x => x.Settings.Name);
        }

        private bool IsDefaultTenant()
        {
            return String.Equals(_currentTenantSettings.Name, TenantHelper.DefaultTenantName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
