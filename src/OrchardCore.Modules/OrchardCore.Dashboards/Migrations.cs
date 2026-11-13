using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Dashboards.Indexes;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;
using YesSql.Sql;

namespace OrchardCore.Dashboards;

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
        await SchemaBuilder.CreateMapIndexTableAsync<DashboardWidgetIndex>(table => table
            .Column<string>("DashboardType", column => column.Nullable().WithLength(DashboardWidgetIndex.MaxDashboardTypeSize))
            .Column<string>("OwnerId", column => column.Nullable().WithLength(ContentItemIndex.MaxOwnerSize))
            .Column<double>("Position")
        );

        await SchemaBuilder.AlterIndexTableAsync<DashboardWidgetIndex>(table => table
            .CreateIndex("IDX_DashboardWidgetIndex_DocumentId",
                "DocumentId",
                "DashboardType",
                "OwnerId",
                "Position")
        );

        // The Admin Dashboard created this widget type before the Dashboards feature existed, don't alter it.
        if (await _contentDefinitionManager.GetTypeDefinitionAsync("HtmlDashboardWidget") is null)
        {
            await _recipeMigrator.ExecuteAsync($"dashboard-widgets{RecipesConstants.RecipeExtension}", this);
        }

        return 1;
    }
}
