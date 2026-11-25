using OrchardCore.Data.Migration;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Themes.TheAdmin.Migrations;

/// <summary>
/// Imports the theme's schemes, and makes Gray the site's default scheme, from the 'admin-schemes' recipe of the
/// theme's Migrations folder: once, when the theme is enabled (during the setup of a site), or on the first start of a
/// site that had the theme enabled before schemes existed. Admins edit and delete them from there; they are never
/// imported again, so a deleted scheme stays deleted.
/// </summary>
public sealed class AdminSchemesMigrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;

    public AdminSchemesMigrations(IRecipeMigrator recipeMigrator)
    {
        _recipeMigrator = recipeMigrator;
    }

    public async Task<int> CreateAsync()
    {
        await _recipeMigrator.ExecuteAsync($"admin-schemes{RecipesConstants.RecipeExtension}", this);

        return 1;
    }
}
