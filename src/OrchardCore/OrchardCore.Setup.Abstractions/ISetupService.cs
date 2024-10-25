using OrchardCore.Recipes.Models;

namespace OrchardCore.Setup.Services;

/// <summary>
/// Contract that represents the setup service.
/// </summary>
public interface ISetupService
{
    /// <summary>
    /// Gets the setup recipes.
    /// </summary>
    /// <returns>A list of <see cref="RecipeDescriptor"/>s.</returns>
    Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync();

    /// <summary>
    /// Set up the tenant.
    /// </summary>
    /// <param name="context">The <see cref="SetupContext"/>.</param>
    /// <returns> A GUID string the represents a setup execution identifier.</returns>
    Task<string> SetupAsync(SetupContext context);
}
