using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.Records
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
            _contentDefinitionManager.AlterPartDefinition(nameof(LocalizationPart), builder => builder
                .Attachable()
                .WithDescription("Provides a way to create localized version of content."));

            SchemaBuilder.CreateMapIndexTable(nameof(LocalizedContentItemIndex), table => table
                .Column<string>("LocalizationSet", col => col.WithLength(26))
                .Column<string>("Culture", col => col.WithLength(4))
                .Column<string>("ContentItemId", c => c.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(LocalizedContentItemIndex), table => table
              .CreateIndex("IDX_LocalizationPartIndex_ContentItemId", "ContentItemId")
            );

            return 1;
        }
    }
}
