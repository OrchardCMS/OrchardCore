using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Recipes.Services;
using OrchardCore.Routing;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Controllers
{
    public class AdminController : Controller
    {
        private readonly IShellHost _shellHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IEnumerable<DatabaseProvider> _databaseProviders;
        private readonly IAuthorizationService _authorizationService;
        private readonly ShellSettings _currentShellSettings;
        private readonly IFeatureProfilesService _featureProfilesService;
        private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;
        private readonly IDataProtectionProvider _dataProtectorProvider;
        private readonly IClock _clock;
        private readonly INotifier _notifier;
        private readonly ITenantValidator _tenantValidator;
        private readonly PagerOptions _pagerOptions;
        private readonly dynamic New;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public AdminController(
            IShellHost shellHost,
            IShellSettingsManager shellSettingsManager,
            IEnumerable<DatabaseProvider> databaseProviders,
            IAuthorizationService authorizationService,
            ShellSettings currentShellSettings,
            IFeatureProfilesService featureProfilesService,
            IEnumerable<IRecipeHarvester> recipeHarvesters,
            IDataProtectionProvider dataProtectorProvider,
            IClock clock,
            INotifier notifier,
            ITenantValidator tenantValidator,
            IOptions<PagerOptions> pagerOptions,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer)
        {
            _shellHost = shellHost;
            _shellSettingsManager = shellSettingsManager;
            _databaseProviders = databaseProviders;
            _authorizationService = authorizationService;
            _currentShellSettings = currentShellSettings;
            _featureProfilesService = featureProfilesService;
            _recipeHarvesters = recipeHarvesters;
            _dataProtectorProvider = dataProtectorProvider;
            _clock = clock;
            _notifier = notifier;
            _tenantValidator = tenantValidator;
            _pagerOptions = pagerOptions.Value;

            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Index(TenantIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Forbid();
            }

            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            var allSettings = _shellHost.GetAllSettings().OrderBy(s => s.Name);
            var dataProtector = _dataProtectorProvider.CreateProtector("Tokens").ToTimeLimitedDataProtector();

            var pager = new Pager(pagerParameters, _pagerOptions.PageSize);

            var entries = allSettings.Select(x =>
               {
                   var entry = new ShellSettingsEntry
                   {
                       Category = x["Category"],
                       Description = x["Description"],
                       Name = x.Name,
                       ShellSettings = x,
                       IsDefaultTenant = x.IsDefaultShell(),
                   };

                   if (x.State == TenantState.Uninitialized && !String.IsNullOrEmpty(x["Secret"]))
                   {
                       entry.Token = dataProtector.Protect(x["Secret"], _clock.UtcNow.Add(new TimeSpan(24, 0, 0)));
                   }

                   return entry;
               }).ToList();

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                entries = entries.Where(t => t.Name.IndexOf(options.Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                    (t.ShellSettings != null &&
                     ((t.ShellSettings.RequestUrlHost != null && t.ShellSettings.RequestUrlHost.IndexOf(options.Search, StringComparison.OrdinalIgnoreCase) > -1) ||
                     (t.ShellSettings.RequestUrlPrefix != null && t.ShellSettings.RequestUrlPrefix.IndexOf(options.Search, StringComparison.OrdinalIgnoreCase) > -1)))).ToList();
            }

            if (!String.IsNullOrWhiteSpace(options.Category))
            {
                entries = entries.Where(t => t.Category?.Equals(options.Category, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            switch (options.Status)
            {
                case TenantsState.Disabled:
                    entries = entries.Where(t => t.ShellSettings.State == TenantState.Disabled).ToList();
                    break;
                case TenantsState.Running:
                    entries = entries.Where(t => t.ShellSettings.State == TenantState.Running).ToList();
                    break;
                case TenantsState.Uninitialized:
                    entries = entries.Where(t => t.ShellSettings.State == TenantState.Uninitialized).ToList();
                    break;
            }

            switch (options.OrderBy)
            {
                case TenantsOrder.Name:
                    entries = entries.OrderBy(t => t.Name).ToList();
                    break;
                case TenantsOrder.State:
                    entries = entries.OrderBy(t => t.ShellSettings?.State).ToList();
                    break;
                default:
                    entries = entries.OrderByDescending(t => t.Name).ToList();
                    break;
            }
            var count = entries.Count();

            var results = entries
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize).ToList();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Category", options.Category);
            routeData.Values.Add("Options.Status", options.Status);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.OrderBy", options.OrderBy);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var model = new AdminIndexViewModel
            {
                ShellSettingsEntries = results,
                Options = options,
                Pager = pagerShape
            };

            // We populate the SelectLists
            model.Options.TenantsCategories = allSettings
                .GroupBy(t => t["Category"])
                .Where(t => !String.IsNullOrEmpty(t.Key))
                .Select(t => new SelectListItem(t.Key, t.Key, String.Equals(options.Category, t.Key, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            model.Options.TenantsCategories.Insert(0, new SelectListItem(
                S["All"],
                String.Empty,
                selected: String.IsNullOrEmpty(options.Category)));

            model.Options.TenantsStates = new List<SelectListItem>() {
                new SelectListItem() { Text = S["All states"], Value = nameof(TenantsState.All) },
                new SelectListItem() { Text = S["Running"], Value = nameof(TenantsState.Running) },
                new SelectListItem() { Text = S["Disabled"], Value = nameof(TenantsState.Disabled) },
                new SelectListItem() { Text = S["Uninitialized"], Value = nameof(TenantsState.Uninitialized) }
            };

            model.Options.TenantsSorts = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Name"], Value = nameof(TenantsOrder.Name) },
                new SelectListItem() { Text = S["State"], Value = nameof(TenantsOrder.State) }
            };

            model.Options.TenantsBulkAction = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Disable"], Value = nameof(TenantsBulkAction.Disable) },
                new SelectListItem() { Text = S["Enable"], Value = nameof(TenantsBulkAction.Enable) }
            };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(AdminIndexViewModel model)
        {
            return RedirectToAction("Index", new RouteValueDictionary {
                { "Options.Category", model.Options.Category },
                { "Options.Status", model.Options.Status },
                { "Options.OrderBy", model.Options.OrderBy },
                { "Options.Search", model.Options.Search },
                { "Options.TenantsStates", model.Options.TenantsStates }
            });
        }

        [HttpPost]
        [FormValueRequired("submit.BulkAction")]
        public async Task<IActionResult> Index(BulkActionViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Forbid();
            }

            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            var allSettings = _shellHost.GetAllSettings();

            foreach (var tenantName in model.TenantNames ?? Enumerable.Empty<string>())
            {
                if (!_shellHost.TryGetSettings(tenantName, out var shellSettings))
                {
                    break;
                }

                switch (model.BulkAction.ToString())
                {
                    case "Disable":
                        if (shellSettings.IsDefaultShell())
                        {
                            await _notifier.WarningAsync(H["You cannot disable the default tenant."]);
                        }
                        else if (shellSettings.State != TenantState.Running)
                        {
                            await _notifier.WarningAsync(H["The tenant '{0}' is already disabled.", shellSettings.Name]);
                        }
                        else
                        {
                            shellSettings.State = TenantState.Disabled;
                            await _shellHost.UpdateShellSettingsAsync(shellSettings);
                        }

                        break;

                    case "Enable":
                        if (shellSettings.State != TenantState.Disabled)
                        {
                            await _notifier.WarningAsync(H["The tenant '{0}' is already enabled.", shellSettings.Name]);
                        }
                        else
                        {
                            shellSettings.State = TenantState.Running;
                            await _shellHost.UpdateShellSettingsAsync(shellSettings);
                        }

                        break;

                    default:
                        break;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Forbid();
            }

            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }


            var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x).Where(x => x.IsSetupRecipe).OrderBy(r => r.DisplayName).ToArray();

            // Creates a default shell settings based on the configuration.
            var shellSettings = _shellSettingsManager.CreateDefaultSettings();

            var currentFeatureProfile = shellSettings["FeatureProfile"];

            var featureProfiles = await GetFeatureProfilesAsync(currentFeatureProfile);

            var model = new EditTenantViewModel
            {
                Recipes = recipes,
                RequestUrlHost = shellSettings.RequestUrlHost,
                RequestUrlPrefix = shellSettings.RequestUrlPrefix,
                TablePrefix = shellSettings["TablePrefix"],
                RecipeName = shellSettings["RecipeName"],
                FeatureProfile = currentFeatureProfile,
                FeatureProfiles = featureProfiles
            };
            SetConfigurationShellValues(model);

            model.Recipes = recipes;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EditTenantViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Forbid();
            }

            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                await ValidateViewModelAsync(model, true);
            }

            if (ModelState.IsValid)
            {
                // Creates a default shell settings based on the configuration.
                var shellSettings = _shellSettingsManager.CreateDefaultSettings();

                shellSettings.Name = model.Name;
                shellSettings.RequestUrlHost = model.RequestUrlHost;
                shellSettings.RequestUrlPrefix = model.RequestUrlPrefix;
                shellSettings.State = TenantState.Uninitialized;

                SetConfigurationShellValues(model);
                shellSettings["Category"] = model.Category;
                shellSettings["Description"] = model.Description;
                shellSettings["ConnectionString"] = model.ConnectionString;
                shellSettings["TablePrefix"] = model.TablePrefix;
                shellSettings["DatabaseProvider"] = model.DatabaseProvider;
                shellSettings["Secret"] = Guid.NewGuid().ToString();
                shellSettings["RecipeName"] = model.RecipeName;
                shellSettings["FeatureProfile"] = model.FeatureProfile;

                await _shellHost.UpdateShellSettingsAsync(shellSettings);

                return RedirectToAction(nameof(Index));
            }

            var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x).Where(x => x.IsSetupRecipe).OrderBy(r => r.DisplayName).ToArray();
            model.Recipes = recipes;
            model.FeatureProfiles = await GetFeatureProfilesAsync(model.FeatureProfile);

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Forbid();
            }

            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            if (!_shellHost.TryGetSettings(id, out var shellSettings))
            {
                return NotFound();
            }

            var currentFeatureProfile = shellSettings["FeatureProfile"];

            var featureProfiles = await GetFeatureProfilesAsync(currentFeatureProfile);

            var model = new EditTenantViewModel
            {
                Category = shellSettings["Category"],
                Description = shellSettings["Description"],
                Name = shellSettings.Name,
                RequestUrlHost = shellSettings.RequestUrlHost,
                RequestUrlPrefix = shellSettings.RequestUrlPrefix,
                FeatureProfile = currentFeatureProfile,
                FeatureProfiles = featureProfiles
            };

            // The user can change the 'preset' database information only if the
            // tenant has not been initialized yet
            if (shellSettings.State == TenantState.Uninitialized)
            {
                var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
                var recipes = recipeCollections.SelectMany(x => x).Where(x => x.IsSetupRecipe).OrderBy(r => r.DisplayName).ToArray();
                model.Recipes = recipes;

                model.DatabaseProvider = shellSettings["DatabaseProvider"];
                model.TablePrefix = shellSettings["TablePrefix"];
                model.ConnectionString = shellSettings["ConnectionString"];
                model.RecipeName = shellSettings["RecipeName"];
                model.CanEditDatabasePresets = true;
                SetConfigurationShellValues(model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditTenantViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Forbid();
            }

            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                await ValidateViewModelAsync(model, false);
            }

            if (!_shellHost.TryGetSettings(model.Name, out var shellSettings))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                shellSettings["Description"] = model.Description;
                shellSettings["Category"] = model.Category;
                shellSettings.RequestUrlPrefix = model.RequestUrlPrefix;
                shellSettings.RequestUrlHost = model.RequestUrlHost;
                shellSettings["FeatureProfile"] = model.FeatureProfile;

                // The user can change the 'preset' database information only if the
                // tenant has not been initialized yet
                if (shellSettings.State == TenantState.Uninitialized)
                {
                    SetConfigurationShellValues(model);
                    shellSettings["DatabaseProvider"] = model.DatabaseProvider;
                    shellSettings["TablePrefix"] = model.TablePrefix;
                    shellSettings["ConnectionString"] = model.ConnectionString;
                    shellSettings["RecipeName"] = model.RecipeName;
                    shellSettings["Secret"] = Guid.NewGuid().ToString();
                }

                await _shellHost.UpdateShellSettingsAsync(shellSettings);

                return RedirectToAction(nameof(Index));
            }

            // The user can change the 'preset' database information only if the
            // tenant has not been initialized yet
            if (shellSettings.State == TenantState.Uninitialized)
            {
                model.DatabaseProvider = shellSettings["DatabaseProvider"];
                model.TablePrefix = shellSettings["TablePrefix"];
                model.ConnectionString = shellSettings["ConnectionString"];
                model.RecipeName = shellSettings["RecipeName"];
                model.CanEditDatabasePresets = true;
                SetConfigurationShellValues(model);
            }

            var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x).Where(x => x.IsSetupRecipe).OrderBy(r => r.DisplayName).ToArray();
            model.Recipes = recipes;
            model.FeatureProfiles = await GetFeatureProfilesAsync(model.FeatureProfile);

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Forbid();
            }

            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            if (!_shellHost.TryGetSettings(id, out var shellSettings))
            {
                return NotFound();
            }

            if (shellSettings.IsDefaultShell())
            {
                await _notifier.ErrorAsync(H["You cannot disable the default tenant."]);
                return RedirectToAction(nameof(Index));
            }

            if (shellSettings.State != TenantState.Running)
            {
                await _notifier.ErrorAsync(H["You can only disable an Enabled tenant."]);
                return RedirectToAction(nameof(Index));
            }

            shellSettings.State = TenantState.Disabled;
            await _shellHost.UpdateShellSettingsAsync(shellSettings);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Forbid();
            }

            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            if (!_shellHost.TryGetSettings(id, out var shellSettings))
            {
                return NotFound();
            }

            if (shellSettings.State != TenantState.Disabled)
            {
                await _notifier.ErrorAsync(H["You can only enable a Disabled tenant."]);
            }

            shellSettings.State = TenantState.Running;
            await _shellHost.UpdateShellSettingsAsync(shellSettings);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reload(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Forbid();
            }

            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            if (!_shellHost.TryGetSettings(id, out var shellSettings))
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var redirectUrl = Url.Action(nameof(Index));

            await _shellHost.ReloadShellContextAsync(shellSettings);

            return Redirect(redirectUrl);
        }

        private void SetConfigurationShellValues(EditTenantViewModel model)
        {
            var shellSettings = _shellSettingsManager.CreateDefaultSettings();
            var configurationShellConnectionString = shellSettings["ConnectionString"];
            var configurationDatabaseProvider = shellSettings["DatabaseProvider"];

            model.DatabaseConfigurationPreset = !string.IsNullOrEmpty(configurationShellConnectionString) || !string.IsNullOrEmpty(configurationDatabaseProvider);

            if (!string.IsNullOrEmpty(configurationShellConnectionString))
            {
                model.ConnectionString = configurationShellConnectionString;
            }

            if (!string.IsNullOrEmpty(configurationDatabaseProvider))
            {
                model.DatabaseProvider = configurationDatabaseProvider;
            }
        }

        private async Task<List<SelectListItem>> GetFeatureProfilesAsync(string currentFeatureProfile)
        {
            var featureProfiles = (await _featureProfilesService.GetFeatureProfilesAsync())
                .Select(x => new SelectListItem(x.Key, x.Key, String.Equals(x.Key, currentFeatureProfile, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (featureProfiles.Any())
            {
                featureProfiles.Insert(0, new SelectListItem(S["None"], String.Empty, currentFeatureProfile == String.Empty));
            }

            return featureProfiles;
        }

        private async Task ValidateViewModelAsync(EditTenantViewModel model, bool newTenant)
        {
            model.IsNewTenant = newTenant;

            ModelState.AddModelErrors(await _tenantValidator.ValidateAsync(model));
        }
    }
}
