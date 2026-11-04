using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment.Services;

/// <summary>Builds deployment archives using the registered deployment sources.</summary>
public interface IDeploymentArchiveService
{
    /// <summary>Creates a ZIP stream. Disposing the returned stream removes its temporary archive.</summary>
    Task<Stream> CreateAsync(DeploymentPlan plan, RecipeDescriptor recipe);
}
