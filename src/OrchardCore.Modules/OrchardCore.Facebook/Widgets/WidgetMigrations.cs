using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Facebook.Widgets.Models;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Widgets;

public sealed class WidgetMigrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public WidgetMigrations(
        IRecipeMigrator recipeMigrator,
        IContentDefinitionManager contentDefinitionManager)
    {
        _recipeMigrator = recipeMigrator;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(FacebookPluginPart), builder => builder
            .Attachable()
            .WithDescription("Provides a Facebook plugin part to create Facebook social plugin widgets."));

        await _recipeMigrator.ExecuteAsync($"Widgets/migration{RecipesConstants.RecipeExtension}", this);

        return 1;
    }
}
