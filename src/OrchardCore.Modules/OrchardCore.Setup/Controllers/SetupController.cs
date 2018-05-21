using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Services;
using OrchardCore.Setup.ViewModels;

namespace OrchardCore.Setup.Controllers
{
    public class SetupController : Controller
    {
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private const string DefaultRecipe = "Default";
        private readonly IEnumerable<DatabaseProvider> _databaseProviders;
        private readonly IClock _clock;

        public SetupController(
            ISetupService setupService,
            ShellSettings shellSettings,
            IEnumerable<DatabaseProvider> databaseProviders,
            IStringLocalizer<SetupController> t,
            IClock clock)
        {
            _setupService = setupService;
            _shellSettings = shellSettings;
            _databaseProviders = databaseProviders;
            _clock = clock;

            T = t;
        }

        public IStringLocalizer T { get; set; }

        public async Task<ActionResult> Index()
        {
            var recipes = await _setupService.GetSetupRecipesAsync();
            var defaultRecipe = recipes.FirstOrDefault(x => x.Tags.Contains("default")) ?? recipes.FirstOrDefault();

            var model = new SetupViewModel
            {
                DatabaseProviders = _databaseProviders,
                Recipes = recipes,
                RecipeName = defaultRecipe?.Name
            };

            if (!String.IsNullOrEmpty(_shellSettings.ConnectionString))
            {
                model.ConnectionStringPreset = true;
                model.ConnectionString = _shellSettings.ConnectionString;
            }

            if (!String.IsNullOrEmpty(_shellSettings.DatabaseProvider))
            {
                model.DatabaseProviderPreset = true;
                model.DatabaseProvider = _shellSettings.DatabaseProvider;
            }

            if (!String.IsNullOrEmpty(_shellSettings.TablePrefix))
            {
                model.TablePrefixPreset = true;
                model.TablePrefix = _shellSettings.TablePrefix;
            }

            model.TimeZones = _clock.GetTimeZones();

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public async Task<ActionResult> IndexPOST(SetupViewModel model)
        {
            model.DatabaseProviders = _databaseProviders;
            model.Recipes = await _setupService.GetSetupRecipesAsync();

            var selectedProvider = model.DatabaseProviders.FirstOrDefault(x => x.Value == model.DatabaseProvider);

            if (selectedProvider != null && selectedProvider.HasConnectionString && String.IsNullOrWhiteSpace(model.ConnectionString))
            {
                ModelState.AddModelError(nameof(model.ConnectionString), T["The connection string is mandatory for this provider."]);
            }

            if (String.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), T["The password is required."]);
            }

            if (model.Password != model.PasswordConfirmation)
            {
                ModelState.AddModelError(nameof(model.PasswordConfirmation), T["The password confirmation doesn't match the password."]);
            }

            RecipeDescriptor selectedRecipe = null;

            if (String.IsNullOrEmpty(model.RecipeName) || (selectedRecipe = model.Recipes.FirstOrDefault(x => x.Name == model.RecipeName)) == null)
            {
                ModelState.AddModelError(nameof(model.RecipeName), T["Invalid recipe."]);
            }

            if (!String.IsNullOrEmpty(_shellSettings.ConnectionString))
            {
                model.ConnectionStringPreset = true;
                model.ConnectionString = _shellSettings.ConnectionString;
            }

            if (!String.IsNullOrEmpty(_shellSettings.DatabaseProvider))
            {
                model.DatabaseProviderPreset = true;
                model.DatabaseProvider = _shellSettings.DatabaseProvider;
            }

            if (!String.IsNullOrEmpty(_shellSettings.TablePrefix))
            {
                model.TablePrefixPreset = true;
                model.TablePrefix = _shellSettings.TablePrefix;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var setupContext = new SetupContext
            {
                SiteName = model.SiteName,
                EnabledFeatures = null, // default list,
                AdminUsername = model.UserName,
                AdminEmail = model.Email,
                AdminPassword = model.Password,
                Errors = new Dictionary<string, string>(),
                Recipe = selectedRecipe,
                SiteTimeZone = model.SiteTimeZone
            };

            if (!model.DatabaseProviderPreset)
            {
                setupContext.DatabaseProvider = model.DatabaseProvider;
            }

            if (!model.ConnectionStringPreset)
            {
                setupContext.DatabaseConnectionString = model.ConnectionString;
            }

            if (!model.TablePrefixPreset)
            {
                setupContext.DatabaseTablePrefix = model.TablePrefix;
            }

            var executionId = await _setupService.SetupAsync(setupContext);

            // Check if a component in the Setup failed
            if (setupContext.Errors.Any())
            {
                foreach (var error in setupContext.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }

                return View(model);
            }

            return Redirect("~/");
        }
    }
}