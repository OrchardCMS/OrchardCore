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
        private IContentDefinitionManager _contentDefinitionManager;

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
                .Column<string>("ContainedContentItemId", c => c.WithLength(26))
                .Column<string>("JsonPath", c => c.Unlimited())
                .Column<string>("Path", col => col.WithLength(AutoroutePartDisplay.MaxPathLength))
                .Column<bool>("Published")
                .Column<bool>("Latest")
            );

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .CreateIndex("IDX_AutoroutePartIndex_ContentItemIds", "ContentItemId", "ContainedContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .CreateIndex("IDX_AutoroutePartIndex_State", "Published", "Latest")
            );

            // Return 4 to shortcut the second migration on new content definition schemas.
            return 4;
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

        // This code can be removed in a later version.
        public int UpdateFrom3()
        {
            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .AddColumn<string>("ContainedContentItemId", c => c.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .AddColumn<string>("JsonPath", c => c.Unlimited())
            );

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .AddColumn<bool>("Latest", c => c.WithDefault(false))
            );

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .DropIndex("IDX_AutoroutePartIndex_ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .CreateIndex("IDX_AutoroutePartIndex_ContentItemIds", "ContentItemId", "ContainedContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .DropIndex("IDX_AutoroutePartIndex_Published")
            );

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .CreateIndex("IDX_AutoroutePartIndex_State", "Published", "Latest")
            );

            return 4;
        }
    }
}
