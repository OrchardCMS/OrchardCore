using Microsoft.AspNetCore.Mvc;
using Orchard.Localization;
using Orchard.Environment.Shell;
using Orchard.Setup.Services;
using Orchard.Setup.ViewModels;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Localization;
using Orchard.Recipes.Services;

namespace Orchard.Setup.Controllers
{
    public class SetupController : Controller
    {
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private const string DefaultRecipe = "Default";

        public SetupController(
            ISetupService setupService,
            ShellSettings shellSettings,
            IHtmlLocalizer<SetupController> localizer)
        {
            _setupService = setupService;
            _shellSettings = shellSettings;

            T = localizer;
        }

        public IHtmlLocalizer T { get; }

        private ActionResult IndexViewResult(SetupViewModel model)
        {
            return View(model);
        }

        public ActionResult Index()
        {
            var initialSettings = _setupService.Prime();
            var recipes = _setupService.Recipes();
            string recipeDescription = null;

            if (recipes.Count > 0)
            {
                recipeDescription = recipes[0].Description;
            }

            return IndexViewResult(new SetupViewModel
            {
                Recipes = recipes,
                RecipeDescription = recipeDescription
            });
        }

        [HttpPost, ActionName("Index")]
        public async Task<ActionResult> IndexPOST(SetupViewModel model)
        {
            var recipes = _setupService.Recipes();

            if (model.RecipeName == null)
            {
                if (!(recipes.Select(r => r.Name).Contains(DefaultRecipe)))
                {
                    ModelState.AddModelError("Recipe", T["No recipes were found."].Value);
                }
                else {
                    model.RecipeName = DefaultRecipe;
                }
            }

            if (!ModelState.IsValid)
            {
                model.Recipes = recipes;
                model.RecipeDescription = recipes.GetRecipeByName(model.RecipeName).Description;

                return IndexViewResult(model);
            }

            var recipe = recipes.GetRecipeByName(model.RecipeName);
            var setupContext = new SetupContext
            {
                SiteName = model.SiteName,
                DatabaseProvider = model.DatabaseProvider,
                DatabaseConnectionString = model.ConnectionString,
                DatabaseTablePrefix = model.TablePrefix,
                EnabledFeatures = null, // default list,
                AdminUsername = model.AdminUserName,
                AdminEmail = model.AdminEmail,
                AdminPassword = model.Password,
                Recipe = recipe
            };

            var executionId = await _setupService.SetupAsync(setupContext);

            var urlPrefix = "";
            if (!string.IsNullOrWhiteSpace(_shellSettings.RequestUrlPrefix))
            {
                urlPrefix = _shellSettings.RequestUrlPrefix + "/";
            }

            // Redirect to the welcome page.
            // TODO: Redirect on the home page once we don't rely on Orchard.Demo
            return Redirect("~/" + urlPrefix + "home/index");
        }
    }
}