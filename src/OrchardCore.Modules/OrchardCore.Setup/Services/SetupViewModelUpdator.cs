using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.ViewModels;

namespace OrchardCore.Setup.Services
{
    public interface ISetupViewModelUpdator
    {
        Task<IDictionary<string, string>> Update(SetupViewModel model);
    }

    public class SetupViewModelUpdator : ISetupViewModelUpdator
    {
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private const string DefaultRecipe = "Default";
        private readonly IEnumerable<DatabaseProvider> _databaseProviders;

        public SetupViewModelUpdator(
            ISetupService setupService,
            ShellSettings shellSettings,
            IEnumerable<DatabaseProvider> databaseProviders,
            IStringLocalizer<SetupViewModelUpdator> t)
        {
            _setupService = setupService;
            _shellSettings = shellSettings;
            _databaseProviders = databaseProviders;

            T = t;
        }

        public IStringLocalizer T { get; set; }

        public async Task<IDictionary<string, string>> Update(SetupViewModel model)
        {
            var modelState = new Dictionary<string, string>();

            model.DatabaseProviders = _databaseProviders;
            model.Recipes = await _setupService.GetSetupRecipesAsync();

            var selectedProvider = model.DatabaseProviders.FirstOrDefault(x => x.Value == model.DatabaseProvider);

            if (selectedProvider != null && selectedProvider.HasConnectionString && String.IsNullOrWhiteSpace(model.ConnectionString))
            {
                modelState.Add(nameof(model.ConnectionString), T["The connection string is mandatory for this provider."]);
            }

            if (String.IsNullOrEmpty(model.Password))
            {
                modelState.Add(nameof(model.Password), T["The password is required."]);
            }

            if (model.Password != model.PasswordConfirmation)
            {
                modelState.Add(nameof(model.PasswordConfirmation), T["The password confirmation doesn't match the password."]);
            }

            RecipeDescriptor selectedRecipe = null;

            if (String.IsNullOrEmpty(model.RecipeName) || (selectedRecipe = model.Recipes.FirstOrDefault(x => x.Name == model.RecipeName)) == null)
            {
                modelState.Add(nameof(model.RecipeName), T["Invalid recipe."]);
            }

            if (!String.IsNullOrEmpty(_shellSettings.ConnectionString))
            {
                model.DatabaseConfigurationPreset = true;
                model.ConnectionString = _shellSettings.ConnectionString;
            }

            if (!String.IsNullOrEmpty(_shellSettings.DatabaseProvider))
            {
                model.DatabaseConfigurationPreset = true;
                model.DatabaseProvider = _shellSettings.DatabaseProvider;
            }

            if (!String.IsNullOrEmpty(_shellSettings.TablePrefix))
            {
                model.DatabaseConfigurationPreset = true;
                model.TablePrefix = _shellSettings.TablePrefix;
            }

            return modelState;
        }
    }
}
