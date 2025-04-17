using OrchardCore.Entities;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Setup.Services;

/// <summary>
/// Represents a class that generates unique user id for setup that will be stored in <see cref="RecipeEnvironmentFeature"/>.
/// </summary>
/// <remarks>
/// The generated user id will be used to keep track the admin user id during the setup process.
/// </remarks>
public class SetupUserIdGenerator : ISetupUserIdGenerator
{
    private readonly IIdGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupUserIdGenerator"/> class.
    /// </summary>
    /// <param name="generator"></param>
    public SetupUserIdGenerator(IIdGenerator generator) => _generator = generator;

    /// <inheritdoc/>
    public string GenerateUniqueId() => _generator.GenerateUniqueId();
}
