using OrchardCore.Data.Migration;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;

namespace OrchardCore.RateLimits.Migrations;

/// <summary>
/// Seeds the default global rate-limit policy when the feature is enabled for the first time.
/// </summary>
public sealed class GlobalRateLimitsMigrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalRateLimitsMigrations"/> class.
    /// </summary>
    /// <param name="recipeMigrator">The recipe migrator used to seed the default policy.</param>
    public GlobalRateLimitsMigrations(IRecipeMigrator recipeMigrator)
    {
        _recipeMigrator = recipeMigrator;
    }

    /// <summary>
    /// Executes the initial migration for the Rate Limits feature.
    /// </summary>
    /// <returns>The migration version number.</returns>
    public async Task<int> CreateAsync()
    {
        await _recipeMigrator.ExecuteAsync($"default-global-policy{RecipesConstants.RecipeExtension}", this);

        return 1;
    }
}
