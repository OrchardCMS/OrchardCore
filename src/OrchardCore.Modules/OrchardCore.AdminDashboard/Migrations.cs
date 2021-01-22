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

            _contentDefinitionManager.AlterPartDefinition("DashboardPart", builder => builder
                .Attachable()
                .WithDescription("Provides a way to add widgets to a dashboard.")
                );

            return 1;
        }

        public async Task<int> UpdateFrom1()
        {
            await _recipeMigrator.ExecuteAsync("dashboard-widgets.recipe.json", this);

            return 2;
        }
    }
}
