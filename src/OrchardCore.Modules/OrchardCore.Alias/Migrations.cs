using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;
using OrchardCore.Alias.Drivers;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Alias
{
    public class Migrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;
        ShellSettings _shellSettings;

        public Migrations(
            IContentDefinitionManager contentDefinitionManager,
            ShellSettings shellSettings
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _shellSettings = shellSettings;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(AliasPart), builder => builder
                .Attachable()
                .WithDescription("Provides a way to define custom aliases for content items."));

            // NOTE: The Alias Length has been upgraded from 64 characters to 1024.
            // For existing SQL databases update the AliasPartIndex tables Alias column length manually. 
            SchemaBuilder.CreateMapIndexTable(nameof(AliasPartIndex), table => table
                .Column<string>("Alias", col => col.WithLength(AliasPartDisplayDriver.MaxAliasLength))
                .Column<string>("ContentItemId", c => c.WithLength(26))
            );

            if (_shellSettings["DatabaseProvider"] != "MySql")
            {
                SchemaBuilder.AlterTable(nameof(AliasPartIndex), table => table
                    .CreateIndex("IDX_AliasPartIndex_Alias", "Alias")
                );
            }

            return 1;
        }
    }
}
