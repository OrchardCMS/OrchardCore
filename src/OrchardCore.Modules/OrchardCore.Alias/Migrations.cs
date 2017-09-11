using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;

namespace OrchardCore.Alias
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
            _contentDefinitionManager.AlterPartDefinition(nameof(AliasPart), builder => builder
                .Attachable()
                .WithDescription("Provides a way to define custom aliases for content items."));

            SchemaBuilder.CreateMapIndexTable(nameof(AliasPartIndex), table => table
                .Column<string>("Alias", col => col.WithLength(64))
                .Column<string>("ContentItemId", c => c.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(AliasPartIndex), table => table
                .CreateIndex("IDX_AliasPartIndex_Alias", "Alias")
            );

            return 1;
        }
    }
}
