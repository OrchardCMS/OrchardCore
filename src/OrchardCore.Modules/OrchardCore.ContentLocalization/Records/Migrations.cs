using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.ContentLocalization.Records
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

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
                .Column<string>("Culture", col => col.WithLength(16))
                .Column<string>("ContentItemId", c => c.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(LocalizedContentItemIndex), table => table
                .CreateIndex("IDX_LocalizationPartIndex_LocalizationSet_Culture", new[] { "LocalizationSet", "Culture" })
            );

            SchemaBuilder.AlterTable(nameof(LocalizedContentItemIndex), table => table
                .CreateIndex("IDX_LocalizationPartIndex_ContentItemId", "ContentItemId")
            );

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable(nameof(LocalizedContentItemIndex), table => table
                .AddColumn<bool>(nameof(LocalizedContentItemIndex.Published)));

            return 2;
        }
    }
}
