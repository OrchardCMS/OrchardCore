using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Lists.Indexes;

namespace Orchard.Lists
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
            _contentDefinitionManager.AlterPartDefinition("ListPart", builder => builder
                .Attachable()
                .WithDescription("Add a list behavior."));

            SchemaBuilder.CreateMapIndexTable(nameof(ContainedPartIndex), table => table
                .Column<string>("ListContentItemId", c => c.WithLength(32))
                .Column<int>("Order")
            );

            return 1;
        }
    }
}