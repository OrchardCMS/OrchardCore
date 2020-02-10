using OrchardCore.Autoroute.Drivers;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
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

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .CreateIndex("IDX_AutoroutePartIndex_Published", "Published")
            );

            // Return 3 to shortcut the second migration on new content definition schemas.
            return 3;
        }

        // Migrate PartSettings. This only needs to run on old content definition schemas.
        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            _contentDefinitionManager.MigratePartSettings<AutoroutePart, AutoroutePartSettings>();

            return 2;
        }

        // This code can be removed in a later version.
        public int UpdateFrom2()
        {
            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .CreateIndex("IDX_AutoroutePartIndex_Published", "Published")
            );

            return 3;
        }
    }
}