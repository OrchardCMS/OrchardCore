using OrchardCore.Alias.Drivers;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Alias
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
            _contentDefinitionManager.AlterPartDefinition(nameof(AliasPart), builder => builder
                .Attachable()
                .WithDescription("Provides a way to define custom aliases for content items."));

            // NOTE: The Alias Length has been upgraded from 64 characters to 767.
            // For existing SQL databases update the AliasPartIndex tables Alias column length manually.
            SchemaBuilder.CreateMapIndexTable(nameof(AliasPartIndex), table => table
                .Column<string>("Alias", col => col.WithLength(AliasPartDisplayDriver.MaxAliasLength))
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<bool>("Latest", c => c.WithDefault(false))
                .Column<bool>("Published", c => c.WithDefault(true))
            );

            SchemaBuilder.AlterTable(nameof(AliasPartIndex), table => table
                .CreateIndex("IDX_AliasPartIndex_Alias", "Alias", "Published", "Latest")
            );

            // Return 2 to shortcut the second migration on new content definition schemas.
            return 2;
        }

        // This code can be removed in a later version as Latest and Published are alterations.
        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable(nameof(AliasPartIndex), table => table
                .AddColumn<bool>("Latest", c => c.WithDefault(false))
            );

            SchemaBuilder.AlterTable(nameof(AliasPartIndex), table => table
                .AddColumn<bool>("Published", c => c.WithDefault(true))
            );

            return 2;
        }
    }
}
