using OrchardCore.AdminDashboard.Indexes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;
using YesSql.Sql;

namespace OrchardCore.AdminDashboard;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
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

        await _contentDefinitionManager.AlterPartDefinitionAsync("DashboardPart", builder => builder
            .Attachable()
            .WithDescription("Provides a way to add widgets to a dashboard.")
            );

        // Shortcut other migration steps on new content definition schemas.
        return 3;
    }

#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom1()
#pragma warning restore CA1822 // Mark members as static
    {
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
        await _contentDefinitionManager.AlterPartDefinitionAsync("DashboardPart", builder => builder
            .Attachable(false)
            );

        return 4;
    }
}
