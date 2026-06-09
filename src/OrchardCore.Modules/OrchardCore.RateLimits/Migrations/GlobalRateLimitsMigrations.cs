using OrchardCore.Data.Migration;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;

namespace OrchardCore.RateLimits.Migrations;

public sealed class GlobalRateLimitsMigrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;

    public GlobalRateLimitsMigrations(IRecipeMigrator recipeMigrator)
    {
        _recipeMigrator = recipeMigrator;
    }

    public async Task<int> CreateAsync()
    {
        await _recipeMigrator.ExecuteAsync($"default-global-policy{RecipesConstants.RecipeExtension}", this);

        return 1;
    }
}
