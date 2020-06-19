using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Recipes.ViewModels;
using OrchardCore.Security;
using OrchardCore.Settings;

namespace OrchardCore.Recipes.Controllers
{
    public class AdminController : Controller
    {
        private readonly ShellSettings _shellSettings;
        private readonly IExtensionManager _extensionManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;
        private readonly INotifier _notifier;
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly ISiteService _siteService;
        private readonly IHtmlLocalizer H;

        public AdminController(
            ShellSettings shellSettings,
            ISiteService siteService,
            IExtensionManager extensionManager,
            IHtmlLocalizer<AdminController> localizer,
            IAuthorizationService authorizationService,
            IEnumerable<IRecipeHarvester> recipeHarvesters,
            IRecipeExecutor recipeExecutor,
            INotifier notifier)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            _recipeExecutor = recipeExecutor;
            _extensionManager = extensionManager;
            _authorizationService = authorizationService;
            _recipeHarvesters = recipeHarvesters;
            _notifier = notifier;
            H = localizer;
        }

        public async Task<ActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, StandardPermissions.SiteOwner))
            {
                return Forbid();
            }

            var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x);

            recipes = recipes.Where(c => !c.Tags.Contains("hidden", StringComparer.InvariantCultureIgnoreCase));

            var features = _extensionManager.GetFeatures();

            var model = recipes.Select(recipe => new RecipeViewModel
            {
                Name = recipe.DisplayName,
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

            var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x);

            var recipe = recipes.FirstOrDefault(c => c.RecipeFileInfo.Name == fileName && c.BasePath == basePath);

            if (recipe == null)
            {
                _notifier.Error(H["Recipe was not found"]);
                return RedirectToAction("Index");
            }

            var site = await _siteService.GetSiteSettingsAsync();
            var executionId = Guid.NewGuid().ToString("n");

            // Set shell state to "Initializing" so that subsequent HTTP requests
            // are responded to with "Service Unavailable" while running the recipe.
            _shellSettings.State = TenantState.Initializing;

            try
            {
                await _recipeExecutor.ExecuteAsync(executionId, recipe, new
                {
                    site.SiteName,
                    AdminUsername = User.Identity.Name,
                },
                CancellationToken.None);
            }
            finally
            {
                // Don't lock the tenant if the recipe fails.
                _shellSettings.State = TenantState.Running;
            }

            _notifier.Success(H["The recipe '{0}' has been run successfully", recipe.Name]);
            return RedirectToAction("Index");
        }
    }
}
