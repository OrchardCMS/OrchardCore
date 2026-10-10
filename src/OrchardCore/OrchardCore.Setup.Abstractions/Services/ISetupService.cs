using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Setup.Services;

/// <summary>
/// Provides methods for retrieving setup recipes and performing tenant setup operations.
/// </summary>
/// <remarks>Implementations of this interface enable the management of application setup processes, including
/// listing available setup recipes and executing tenant setup. Methods are asynchronous and intended for use in
/// scenarios where application initialization or configuration is required.
/// </remarks>
public interface ISetupService
{
    /// <summary>
    /// Asynchronously retrieves a collection of setup recipe descriptors available for configuration.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see
    /// cref="RecipeDescriptor"/> objects describing available setup recipes. The collection will be empty if no setup
    /// recipes are found.</returns>
    Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync();

    /// <summary>
    /// Asynchronously retrieves the setup recipes available to a given tenant, as listed by the setup screen of this
    /// tenant.
    /// </summary>
    /// <remarks>
    /// The setup recipes of a tenant depend on the features of its setup shell, e.g. a setup feature allowed on the
    /// default tenant only is not part of the setup shell of the other tenants. Use this method to list the setup
    /// recipes of a tenant from another tenant, for instance from the default tenant when creating a new tenant.
    /// </remarks>
    /// <param name="shellSettings">The settings of the tenant to retrieve the setup recipes for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see
    /// cref="RecipeDescriptor"/> objects describing the setup recipes available to the tenant.</returns>
    Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync(ShellSettings shellSettings)
        => GetSetupRecipesAsync();

    /// <summary>
    /// Initializes the setup process asynchronously using the specified context and returns a status message upon
    /// completion.
    /// </summary>
    /// <param name="context">The setup context containing configuration and parameters required for initialization. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a string describing the outcome of
    /// the setup process.</returns>
    Task<string> SetupAsync(SetupContext context);
}
