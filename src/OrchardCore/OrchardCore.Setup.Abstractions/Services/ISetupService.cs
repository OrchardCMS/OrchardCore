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
    /// Initializes the setup process asynchronously using the specified context and returns a status message upon
    /// completion.
    /// </summary>
    /// <param name="context">The setup context containing configuration and parameters required for initialization. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a string describing the outcome of
    /// the setup process.</returns>
    Task<string> SetupAsync(SetupContext context);
}
