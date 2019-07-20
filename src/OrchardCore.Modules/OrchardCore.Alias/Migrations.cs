using System.Threading.Tasks;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Alias
{
    public class Migrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task<int> CreateAsync()
        {
            await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(AliasPart), builder => builder
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
