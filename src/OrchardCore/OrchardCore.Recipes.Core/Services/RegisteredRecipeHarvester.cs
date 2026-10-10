using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Finds the recipes registered with <c>AddRecipe()</c> by the enabled features of the tenant.
/// </summary>
public sealed class RegisteredRecipeHarvester : IRecipeHarvester
{
    private readonly RecipeOptions _recipeOptions;
    private readonly IRecipeReader _recipeReader;
    private readonly IExtensionManager _extensionManager;
    private readonly IHostEnvironment _hostingEnvironment;
    private readonly ILogger _logger;

    public RegisteredRecipeHarvester(
        IOptions<RecipeOptions> recipeOptions,
        IRecipeReader recipeReader,
        IExtensionManager extensionManager,
        IHostEnvironment hostingEnvironment,
        ILogger<RegisteredRecipeHarvester> logger)
    {
        _recipeOptions = recipeOptions.Value;
        _recipeReader = recipeReader;
        _extensionManager = extensionManager;
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
    {
        var recipeDescriptors = new List<RecipeDescriptor>();
        var harvestedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var fileProvider = _hostingEnvironment.ContentRootFileProvider;

        foreach (var registration in _recipeOptions.Recipes)
        {
            var root = string.Empty;

            if (!string.IsNullOrEmpty(registration.ExtensionId))
            {
                var extension = _extensionManager.GetExtension(registration.ExtensionId);

                if (extension is null || !extension.Exists)
                {
                    _logger.LogWarning("The recipe '{RecipePath}' is registered for the extension '{ExtensionId}' which doesn't exist.", registration.Path, registration.ExtensionId);

                    continue;
                }

                root = extension.SubPath;
            }

            var relativePath = NormalizePath(registration.Path);
            var folderPath = GetFolderPath(relativePath);

            // The 'Recipes' folder is scanned regardless of the enabled features, so a recipe located there would
            // not depend on the feature registering it. This harvester leaves it to the folder based harvesters.
            if (string.Equals(folderPath, RecipesConstants.RecipesFolderName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "The recipe '{RecipePath}' registered by the feature '{FeatureId}' is located in the '{RecipesFolder}' folder, which is scanned regardless of the enabled features. Move it to a sub folder so that it only depends on the feature registering it.",
                    registration.Path,
                    registration.FeatureId,
                    RecipesConstants.RecipesFolderName);

                continue;
            }

            var filePath = PathExtensions.Combine(root, relativePath);

            if (!harvestedPaths.Add(filePath))
            {
                continue;
            }

            var fileInfo = fileProvider.GetFileInfo(filePath);

            if (!fileInfo.Exists)
            {
                _logger.LogWarning("The recipe file '{RecipePath}' registered by the feature '{FeatureId}' was not found.", filePath, registration.FeatureId);

                continue;
            }

            var recipeDescriptor = await _recipeReader.GetRecipeDescriptorAsync(PathExtensions.Combine(root, folderPath), fileInfo, fileProvider);

            if (recipeDescriptor is null)
            {
                continue;
            }

            recipeDescriptor.FeatureId = registration.FeatureId;

            recipeDescriptors.Add(recipeDescriptor);
        }

        return recipeDescriptors;
    }

    private static string NormalizePath(string path)
        => path.Replace('\\', '/').TrimStart('~').Trim('/');

    private static string GetFolderPath(string path)
    {
        var index = path.LastIndexOf('/');

        return index == -1 ? string.Empty : path[..index];
    }
}
