using OrchardCore.AdminDashboard.Indexes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;
using YesSql.Sql;

namespace OrchardCore.AdminDashboard;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IRecipeMigrator _recipeMigrator;

    public Migrations(
        IContentDefinitionManager contentDefinitionManager,
        IRecipeMigrator recipeMigrator)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _recipeMigrator = recipeMigrator;
    }

    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<DashboardPartIndex>(table => table
           .Column<double>("Position")
        );

        await SchemaBuilder.AlterIndexTableAsync<DashboardPartIndex>(table => table
            .CreateIndex("IDX_DashboardPart_DocumentId",
                "DocumentId",
                "Position")
        );

        await _recipeMigrator.ExecuteAsync($"dashboard-widgets{RecipesConstants.RecipeExtension}", this);

        // Shortcut other migration steps on new content definition schemas.
        return 4;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await _recipeMigrator.ExecuteAsync($"dashboard-widgets{RecipesConstants.RecipeExtension}", this);

        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<DashboardPartIndex>(table => table
            .CreateIndex("IDX_DashboardPart_DocumentId",
                "DocumentId",
                "Position")
        );

        return 3;
    }

    public async Task<int> UpdateFrom3Async()
    {
        await _contentDefinitionManager.DeletePartDefinitionAsync("DashboardPart");

        return 4;
    }
}
