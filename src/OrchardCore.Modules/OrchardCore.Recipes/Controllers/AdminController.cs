using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Recipes.ViewModels;
using OrchardCore.Security;

namespace OrchardCore.Recipes.Controllers
{
    public class AdminController : Controller
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly IEnumerable<IRecipeEnvironmentProvider> _environmentProviders;
        private readonly INotifier _notifier;
        protected readonly IHtmlLocalizer H;
        private readonly ILogger _logger;

        public AdminController(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IShellFeaturesManager shellFeaturesManager,
            IAuthorizationService authorizationService,
            IEnumerable<IRecipeHarvester> recipeHarvesters,
            IRecipeExecutor recipeExecutor,
            IEnumerable<IRecipeEnvironmentProvider> environmentProviders,
            INotifier notifier,
            IHtmlLocalizer<AdminController> localizer,
            ILogger<AdminController> logger)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _shellFeaturesManager = shellFeaturesManager;
            _authorizationService = authorizationService;
            _recipeHarvesters = recipeHarvesters;
            _recipeExecutor = recipeExecutor;
            _environmentProviders = environmentProviders;
            _notifier = notifier;
            H = localizer;
            _logger = logger;
        }

        public async Task<ActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, StandardPermissions.SiteOwner))
            {
                return Forbid();
            }

            var features = await _shellFeaturesManager.GetAvailableFeaturesAsync();
            var recipes = await GetRecipesAsync(features);

            var model = recipes.Select(recipe => new RecipeViewModel
            {
                Name = recipe.Name,
                DisplayName = recipe.DisplayName,
                FileName = recipe.RecipeFileInfo.Name,
                BasePath = recipe.BasePath,
                Tags = recipe.Tags,
                IsSetupRecipe = recipe.IsSetupRecipe,
                Feature = features.FirstOrDefault(f => recipe.BasePath.Contains(f.Extension.SubPath))?.Name ?? "Application",
                Description = recipe.Description
            }).ToArray();

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Execute(string basePath, string fileName)
        {
            if (!await _authorizationService.AuthorizeAsync(User, StandardPermissions.SiteOwner))
            {
                return Forbid();
            }

            var features = await _shellFeaturesManager.GetAvailableFeaturesAsync();
            var recipes = await GetRecipesAsync(features);

            var recipe = recipes.FirstOrDefault(c => c.RecipeFileInfo.Name == fileName && c.BasePath == basePath);

            if (recipe == null)
            {
                await _notifier.ErrorAsync(H["Recipe was not found."]);
                return RedirectToAction(nameof(Index));
            }

            var environment = new Dictionary<string, object>();
            await _environmentProviders.OrderBy(x => x.Order).InvokeAsync((provider, env) => provider.PopulateEnvironmentAsync(env), environment, _logger);

            var executionId = Guid.NewGuid().ToString("n");

            await _recipeExecutor.ExecuteAsync(executionId, recipe, environment, CancellationToken.None);

            await _shellHost.ReleaseShellContextAsync(_shellSettings);

            await _notifier.SuccessAsync(H["The recipe '{0}' has been run successfully.", recipe.DisplayName]);

            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<RecipeDescriptor>> GetRecipesAsync(IEnumerable<IFeatureInfo> features)
        {
            var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x)
                .Where(r => !r.IsSetupRecipe &&
                    (r.Tags == null || !r.Tags.Contains("hidden", StringComparer.InvariantCultureIgnoreCase)) &&
                    features.Any(f => r.BasePath != null && f.Extension?.SubPath != null && r.BasePath.Contains(f.Extension.SubPath, StringComparison.OrdinalIgnoreCase)));

            return recipes;
        }
    }
}
