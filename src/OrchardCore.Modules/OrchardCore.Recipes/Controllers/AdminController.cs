using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Recipes.ViewModels;

namespace OrchardCore.Recipes.Controllers;

[Admin("Recipes/{action}", "Recipes{action}")]
public sealed class AdminController : Controller
{
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;
    private readonly IRecipeExecutor _recipeExecutor;
    private readonly IEnumerable<IRecipeEnvironmentProvider> _environmentProviders;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
        IShellHost shellHost,
        ShellSettings shellSettings,
        IShellFeaturesManager shellFeaturesManager,
        IAuthorizationService authorizationService,
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        IRecipeExecutor recipeExecutor,
        IEnumerable<IRecipeEnvironmentProvider> environmentProviders,
        INotifier notifier,
        ILogger<AdminController> logger,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _shellFeaturesManager = shellFeaturesManager;
        _authorizationService = authorizationService;
        _recipeHarvesters = recipeHarvesters;
        _recipeExecutor = recipeExecutor;
        _environmentProviders = environmentProviders;
        _notifier = notifier;
        _logger = logger;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("Recipes", "Recipes")]
    public async Task<ActionResult> Index(
        [FromServices] IShapeFactory shapeFactory,
        [FromServices] IDisplayManager<RecipeEntry> displayManager,
        [FromServices] IUpdateModelAccessor updateModelAccessor,
        [FromServices] IAdminListService adminListService)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RecipePermissions.ManageRecipes))
        {
            return Forbid();
        }

        var features = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        var recipes = await GetRecipesAsync(features);

        var entries = recipes.Select(recipe => new RecipeEntry
        {
            Name = recipe.Name,
            DisplayName = recipe.DisplayName,
            FileName = recipe.RecipeFileInfo.Name,
            BasePath = recipe.BasePath,
            Tags = recipe.Tags,
            IsSetupRecipe = recipe.IsSetupRecipe,
            Feature = features.FirstOrDefault(f => recipe.BasePath.Contains(f.Extension.SubPath))?.Name ?? "Application",
            Description = recipe.Description,
        }).ToArray();

        var columns = await adminListService.GetColumnsAsync(RecipesAdminList.Name, RecipesAdminList.GetDefaultColumns(S), cancellationToken: HttpContext.RequestAborted);
        var layout = await adminListService.GetLayoutAsync(RecipesAdminList.Name, cancellationToken: HttpContext.RequestAborted);

        var model = new RecipesIndexViewModel();

        // The features share one layout, so the page offers it once, beside its search bar, instead of letting
        // each of its lists carry a selector of its own. Taking the offer here is what stops them.
        var layoutOptions = await adminListService.GetLayoutOptionsAsync(RecipesAdminList.Name, HttpContext.RequestAborted);

        if (layoutOptions.Count > 0)
        {
            model.LayoutSelector = await shapeFactory.CreateAsync(AdminListConstants.LayoutSelectorShapeType, Arguments.From(new
            {
                ListName = RecipesAdminList.Name,
                Current = layout,
                // Not "Items": a shape already exposes that name for its child shapes.
                Layouts = layoutOptions,
            }));
        }

        // The page keeps one list per feature, and every list follows the configured layout.
        foreach (var group in entries.GroupBy(entry => entry.Feature).OrderBy(group => group.Key))
        {
            var rows = new List<object>();

            foreach (var entry in group.OrderBy(entry => entry.DisplayName))
            {
                var shape = await displayManager.BuildDisplayAsync(entry, updateModelAccessor.ModelUpdater, OrchardCoreConstants.DisplayType.SummaryAdmin);

                // The rows carry the attributes used by the client-side search of the list-management script.
                if (shape is Shape rowShape)
                {
                    rowShape.Attributes["data-filter-value"] = group.Key + " " + entry.DisplayName;
                }

                rows.Add(shape);
            }

            model.Groups.Add(new RecipeGroupViewModel
            {
                Feature = group.Key,
                FilterValue = group.Key + " " + string.Join(' ', group.Select(entry => entry.DisplayName)),
                List = await shapeFactory.CreateAsync(AdminListConstants.ShapeType, Arguments.From(new
                {
                    Name = RecipesAdminList.Name,
                    Layout = layout,
                    Columns = columns,
                    Rows = rows,
                    ItemCssClass = "list-group-item",
                })),
            });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Execute(string basePath, string fileName)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RecipePermissions.ManageRecipes))
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

        try
        {
            var executionId = Guid.NewGuid().ToString("n");

            await _recipeExecutor.ExecuteAsync(executionId, recipe, environment, CancellationToken.None);

            await _shellHost.ReleaseShellContextAsync(_shellSettings);

            await _notifier.SuccessAsync(H["The recipe '{0}' has been run successfully.", recipe.DisplayName]);
        }
        catch (RecipeExecutionException e)
        {
            _logger.LogError(e, "Unable to import a recipe file.");

            await _notifier.ErrorAsync(H["The recipe '{0}' failed to run due to the following errors: {1}", recipe.DisplayName, string.Join(' ', e.StepResult.Errors)]);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to import a recipe file.");

            await _notifier.ErrorAsync(H["Unexpected error occurred while running the '{0}' recipe.", recipe.DisplayName]);
        }

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
