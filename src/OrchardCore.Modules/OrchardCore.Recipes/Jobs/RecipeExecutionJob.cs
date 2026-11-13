using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Jobs;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.Jobs;

/// <summary>
/// The parameters of a run of <see cref="RecipeExecutionJob"/>.
/// </summary>
public sealed class RecipeExecutionJobParameters
{
    /// <summary>
    /// Gets or sets the path of the folder of the recipe, as its harvester gives it.
    /// </summary>
    public string BasePath { get; set; }

    /// <summary>
    /// Gets or sets the name of the file of the recipe.
    /// </summary>
    public string FileName { get; set; }
}

/// <summary>
/// Runs a recipe of an enabled feature in the background, when it is run from the admin, so a long recipe doesn't
/// make the request wait. It is registered when the Jobs feature is enabled.
/// </summary>
public sealed class RecipeExecutionJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "RecipeExecution";

    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;
    private readonly IRecipeExecutor _recipeExecutor;
    private readonly IEnumerable<IRecipeEnvironmentProvider> _environmentProviders;
    private readonly ILogger _logger;

    public RecipeExecutionJob(
        IShellHost shellHost,
        ShellSettings shellSettings,
        IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        IRecipeExecutor recipeExecutor,
        IEnumerable<IRecipeEnvironmentProvider> environmentProviders,
        ILogger<RecipeExecutionJob> logger)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _shellFeaturesManager = shellFeaturesManager;
        _recipeHarvesters = recipeHarvesters;
        _recipeExecutor = recipeExecutor;
        _environmentProviders = environmentProviders;
        _logger = logger;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var parameters = context.GetParameters<RecipeExecutionJobParameters>();
        var recipe = await FindRecipeAsync(_recipeHarvesters, _shellFeaturesManager, parameters.BasePath, parameters.FileName)
            ?? throw new InvalidOperationException($"The recipe '{parameters.FileName}' was not found among the recipes of the enabled features.");

        var environment = new Dictionary<string, object>();
        await _environmentProviders.OrderBy(provider => provider.Order).InvokeAsync((provider, env) => provider.PopulateEnvironmentAsync(env), environment, _logger);

        context.ReportProgress(null, $"Running the recipe '{recipe.DisplayName}'.");

        try
        {
            await _recipeExecutor.ExecuteAsync(Guid.NewGuid().ToString("n"), recipe, environment, cancellationToken);
        }
        catch (RecipeExecutionException e)
        {
            foreach (var error in e.StepResult.Errors)
            {
                context.LogError(error);
            }

            throw new InvalidOperationException($"The recipe '{recipe.DisplayName}' failed to run due to the following errors: {string.Join(' ', e.StepResult.Errors)}", e);
        }

        // What the recipe changed, such as the enabled features, applies to the next requests.
        await _shellHost.ReleaseShellContextAsync(_shellSettings);

        context.LogInformation($"The recipe '{recipe.DisplayName}' has been run successfully.");
    }

    /// <summary>
    /// Finds a recipe the admin can run: a recipe of an enabled feature that is neither a setup recipe nor hidden.
    /// </summary>
    internal static async Task<RecipeDescriptor> FindRecipeAsync(IEnumerable<IRecipeHarvester> recipeHarvesters, IShellFeaturesManager shellFeaturesManager, string basePath, string fileName)
    {
        var features = await shellFeaturesManager.GetAvailableFeaturesAsync();
        var recipeCollections = await Task.WhenAll(recipeHarvesters.Select(harvester => harvester.HarvestRecipesAsync()));

        return recipeCollections.SelectMany(recipes => recipes)
            .Where(recipe => !recipe.IsSetupRecipe &&
                (recipe.Tags == null || !recipe.Tags.Contains("hidden", StringComparer.InvariantCultureIgnoreCase)) &&
                features.Any(feature => recipe.BasePath != null && feature.Extension?.SubPath != null && recipe.BasePath.Contains(feature.Extension.SubPath, StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefault(recipe => recipe.RecipeFileInfo.Name == fileName && recipe.BasePath == basePath);
    }
}
