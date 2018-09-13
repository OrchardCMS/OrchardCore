using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Data;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Hosting.ShellBuilders;
using OrchardCore.Modules;
using OrchardCore.Recipes.Services;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Controllers
{
    public class AdminController : Controller
    {
        private readonly IShellHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IEnumerable<DatabaseProvider> _databaseProviders;
        private readonly IAuthorizationService _authorizationService;
        private readonly ShellSettings _currentShellSettings;
        private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;
        private readonly IDataProtectionProvider _dataProtectorProvider;
        private readonly IClock _clock;
        private readonly INotifier _notifier;

        public AdminController(
            IShellHost orchardHost,
            ShellSettings currentShellSettings,
            IAuthorizationService authorizationService,
            IShellSettingsManager shellSettingsManager,
            IEnumerable<DatabaseProvider> databaseProviders,
            IDataProtectionProvider dataProtectorProvider,
            IClock clock,
            INotifier notifier,
            IEnumerable<IRecipeHarvester> recipeHarvesters,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer)
        {
            _dataProtectorProvider = dataProtectorProvider;
            _clock = clock;
            _recipeHarvesters = recipeHarvesters;
            _orchardHost = orchardHost;
            _authorizationService = authorizationService;
            _shellSettingsManager = shellSettingsManager;
            _databaseProviders = databaseProviders;
            _currentShellSettings = currentShellSettings;
            _notifier = notifier;

            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public IStringLocalizer S { get; set; }
        public IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Index()
        {
            var shells = await GetShellsAsync();
            var dataProtector = _dataProtectorProvider.CreateProtector("Tokens").ToTimeLimitedDataProtector();

            var model = new AdminIndexViewModel
            {
                ShellSettingsEntries = shells.Select(x =>
                {
                    var entry = new ShellSettingsEntry
                    {
                        Name = x.Settings.Name,
                        ShellSettings = x.Settings,
                        IsDefaultTenant = string.Equals(x.Settings.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase)
                    };

                    if (x.Settings.State == TenantState.Uninitialized && !string.IsNullOrEmpty(x.Settings.Secret))
                    {
                        entry.Token = dataProtector.Protect(x.Settings.Secret, _clock.UtcNow.Add(new TimeSpan(24, 0, 0)));
                    }

                    return entry;
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

            var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x).Where(x => x.IsSetupRecipe).ToArray();

            var model = new EditTenantViewModel();
            model.Recipes = recipes;

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
                await ValidateViewModel(model, true);
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
                    State = TenantState.Uninitialized,
                    Secret = Guid.NewGuid().ToString(),
                    RecipeName = model.RecipeName
                };

                _shellSettingsManager.SaveSettings(shellSettings);
                var shellContext = await _orchardHost.GetOrCreateShellContextAsync(shellSettings);

                return RedirectToAction(nameof(Index));
            }

            var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x).Where(x => x.IsSetupRecipe).ToArray();
            model.Recipes = recipes;

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

            var shellContext = (await GetShellsAsync())
                .Where(x => string.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
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
                RequestUrlPrefix = shellSettings.RequestUrlPrefix,
            };

            // The user can change the 'preset' database information only if the 
            // tenant has not been initialized yet
            if (shellSettings.State == TenantState.Uninitialized)
            {
                var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
                var recipes = recipeCollections.SelectMany(x => x).Where(x => x.IsSetupRecipe).ToArray();
                model.Recipes = recipes;

                model.DatabaseProvider = shellSettings.DatabaseProvider;
                model.TablePrefix = shellSettings.TablePrefix;
                model.ConnectionString = shellSettings.ConnectionString;
                model.RecipeName = shellSettings.RecipeName;
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
                await ValidateViewModel(model, false);
            }

            var shellContext = (await GetShellsAsync())
                .Where(x => string.Equals(x.Settings.Name, model.Name, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (shellContext == null)
            {
                return NotFound();
            }

            var shellSettings = shellContext.Settings;

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
                    shellSettings.RecipeName = model.RecipeName;
                    shellSettings.Secret = Guid.NewGuid().ToString();
                }

                await _orchardHost.UpdateShellSettingsAsync(shellSettings);

                return RedirectToAction(nameof(Index));
            }

            // The user can change the 'preset' database information only if the 
            // tenant has not been initialized yet
            if (shellSettings.State == TenantState.Uninitialized)
            {
                model.DatabaseProvider = shellSettings.DatabaseProvider;
                model.TablePrefix = shellSettings.TablePrefix;
                model.ConnectionString = shellSettings.ConnectionString;
                model.RecipeName = shellSettings.RecipeName;
                model.CanSetDatabasePresets = true;
            }

            var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x).Where(x => x.IsSetupRecipe).ToArray();
            model.Recipes = recipes;

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

            var shellContext = (await GetShellsAsync())
                .Where(x => string.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (shellContext == null)
            {
                return NotFound();
            }

            var shellSettings = shellContext.Settings;

            if (string.Equals(shellSettings.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase))
            {
                _notifier.Error(H["You cannot disable the default tenant."]);
                return RedirectToAction(nameof(Index));
            }

            if (shellSettings.State != TenantState.Running)
            {
                _notifier.Error(H["You can only disable an Enabled tenant."]);
                return RedirectToAction(nameof(Index));
            }

            shellSettings.State = TenantState.Disabled;
            await _orchardHost.UpdateShellSettingsAsync(shellSettings);

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

            var shellContext = (await _orchardHost.ListShellContextsAsync())
                .OrderBy(x => x.Settings.Name)
                .Where(x => string.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (shellContext == null)
            {
                return NotFound();
            }

            var shellSettings = shellContext.Settings;

            if (shellSettings.State != TenantState.Disabled)
            {
                _notifier.Error(H["You can only enable a Disabled tenant."]);
            }

            shellSettings.State = TenantState.Running;
            await _orchardHost.UpdateShellSettingsAsync(shellSettings);

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

            var shellContext = (await _orchardHost.ListShellContextsAsync())
                .OrderBy(x => x.Settings.Name)
                .Where(x => string.Equals(x.Settings.Name, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (shellContext == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var redirectUrl = Url.Action(nameof(Index));

            var shellSettings = shellContext.Settings;
            await _orchardHost.ReloadShellContextAsync(shellSettings);

            return Redirect(redirectUrl);
        }

        private async Task ValidateViewModel(EditTenantViewModel model, bool newTenant)
        {
            var selectedProvider = _databaseProviders.FirstOrDefault(x => x.Value == model.DatabaseProvider);

            if (selectedProvider != null && selectedProvider.HasConnectionString && String.IsNullOrWhiteSpace(model.ConnectionString))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.ConnectionString), S["The connection string is mandatory for this provider."]);
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["The tenant name is mandatory."]);
            }

            var allShells = await GetShellsAsync();

            if (newTenant && allShells.Any(tenant => string.Equals(tenant.Settings.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
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

            var allOtherShells = allShells.Where(tenant => !string.Equals(tenant.Settings.Name, model.Name, StringComparison.OrdinalIgnoreCase));
            if (allOtherShells.Any(tenant => string.Equals(tenant.Settings.RequestUrlPrefix, model.RequestUrlPrefix?.Trim(), StringComparison.OrdinalIgnoreCase) && string.Equals(tenant.Settings.RequestUrlHost, model.RequestUrlHost, StringComparison.OrdinalIgnoreCase)))
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

        private async Task<IEnumerable<ShellContext>> GetShellsAsync()
        {
            return (await _orchardHost.ListShellContextsAsync()).OrderBy(x => x.Settings.Name);
        }

        private bool IsDefaultShell()
        {
            return string.Equals(_currentShellSettings.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
