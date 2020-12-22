using OrchardCore.Data.Migration;
using OrchardCore.AdminDashboard.Indexes;
using YesSql.Sql;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.AdminDashboard
{
    public class Migrations : DataMigration
    {
        private IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<DashboardPartIndex>(table => {});

            _contentDefinitionManager.AlterPartDefinition("DashboardPart", builder => builder
                .Attachable()
                .WithDescription("Provides a way to add widgets to a dashboard for your content items.")
                );

            return 1;
        }
    }
}
