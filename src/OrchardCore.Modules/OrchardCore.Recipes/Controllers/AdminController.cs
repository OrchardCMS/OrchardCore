using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Recipes.Services;
using OrchardCore.Recipes.ViewModels;
using OrchardCore.Security;
using OrchardCore.Settings;

namespace OrchardCore.Recipes.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly INotifier _notifier;
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly ISiteService _siteService;

        public AdminController(
            ISiteService siteService,
            IAdminThemeService adminThemeService,
            IExtensionManager extensionManager,
            IHtmlLocalizer<AdminController> localizer,
            IAuthorizationService authorizationService,
            IRecipeHarvester recipeHarvester,
            IRecipeExecutor recipeExecutor,
            INotifier notifier)
        {
            _siteService = siteService;
            _recipeExecutor = recipeExecutor;
            _recipeHarvester = recipeHarvester;
            _extensionManager = extensionManager;
            _authorizationService = authorizationService;
            _notifier = notifier;

            T = localizer;
        }

        public IHtmlLocalizer T { get; }

        public async Task<ActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, StandardPermissions.SiteOwner))
            {
                return Unauthorized();
            }

            var recipes = await _recipeHarvester.HarvestRecipesAsync();
            recipes = recipes.Where(c => c.IsSetupRecipe == false);
            var features = _extensionManager.GetFeatures();

            var model = recipes.Select(recipe => new RecipeViewModel
            {
                Name = recipe.Name,
                Path = recipe.RecipeFileInfo.PhysicalPath,
                BasePath = recipe.BasePath,
                Tags = recipe.Tags,
                Feature = features.FirstOrDefault(f=>recipe.BasePath.Contains(f.Extension.SubPath))?.Name,
                Description = recipe.Description
            }).ToArray();

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Execute(string path)
        {
            if (!await _authorizationService.AuthorizeAsync(User, StandardPermissions.SiteOwner))
            {
                return Unauthorized();
            }

            var recipes = await _recipeHarvester.HarvestRecipesAsync();

            var recipe = recipes.FirstOrDefault(c => c.RecipeFileInfo.PhysicalPath == path);

            if (recipe == null)
            {
                _notifier.Error(T["Recipe was not found"]);
                return RedirectToAction("Index");
            }

            var site = await _siteService.GetSiteSettingsAsync();
            var executionId = Guid.NewGuid().ToString("n");

            await _recipeExecutor.ExecuteAsync(executionId, recipe, new
            {
                site.SiteName,
                AdminUsername = User.Identity.Name,
            });

            _notifier.Success(T["The recipe '{0}' has been run successfully", recipe.Name]);
            return RedirectToAction("Index");
        }
    }
}
