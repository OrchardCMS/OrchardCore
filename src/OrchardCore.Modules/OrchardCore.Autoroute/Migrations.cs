using OrchardCore.Autoroute.Drivers;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;

namespace OrchardCore.Autoroute
{
    public class Migrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("AutoroutePart", builder => builder
                .Attachable()
                .WithDescription("Provides a custom url for your content item."));

            SchemaBuilder.CreateMapIndexTable(nameof(AutoroutePartIndex), table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("Path", col => col.WithLength(AutoroutePartDisplay.MaxPathLength))
                .Column<bool>("Published")
            );

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .CreateIndex("IDX_AutoroutePartIndex_ContentItemId", "ContentItemId")
            );

            return 1;
        }
    }
}