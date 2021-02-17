using OrchardCore.Data.Migration;
using OrchardCore.AdminDashboard.Indexes;
using YesSql.Sql;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Recipes.Services;
using System.Threading.Tasks;

namespace OrchardCore.AdminDashboard
{
    public class Migrations : DataMigration
    {
        private IContentDefinitionManager _contentDefinitionManager;
        private readonly IRecipeMigrator _recipeMigrator;

        public Migrations(IContentDefinitionManager contentDefinitionManager, IRecipeMigrator recipeMigrator)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _recipeMigrator = recipeMigrator;
        }

        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<DashboardPartIndex>(table => table
               .Column<double>("Position")
            );

            SchemaBuilder.AlterIndexTable<DashboardPartIndex>(table => table
                .CreateIndex($"IDX_{nameof(DashboardPartIndex)}_{nameof(DashboardPartIndex.Position)}",
                    "DocumentId",
                    nameof(DashboardPartIndex.Position))
            );

            _contentDefinitionManager.AlterPartDefinition("DashboardPart", builder => builder
                .Attachable()
                .WithDescription("Provides a way to add widgets to a dashboard.")
                );

            return 1;
        }

        public async Task<int> UpdateFrom1Async()
        {
            await _recipeMigrator.ExecuteAsync("dashboard-widgets.recipe.json", this);

            // Shortcut other migration steps on new content definition schemas.
            return 3;
        }

        // This code can be removed in a later version.
        public int UpdateFrom2()
        {
            SchemaBuilder.AlterIndexTable<DashboardPartIndex>(table => table
                .CreateIndex($"IDX_{nameof(DashboardPartIndex)}_{nameof(DashboardPartIndex.Position)}",
                    "DocumentId",
                    nameof(DashboardPartIndex.Position))
            );

            return 3;
        }        
    }
}
