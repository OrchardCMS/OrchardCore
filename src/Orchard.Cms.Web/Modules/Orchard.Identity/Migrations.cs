using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Identity.Indexes;
using Orchard.Identity.Models;

namespace Orchard.Identity
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
            _contentDefinitionManager.AlterPartDefinition(nameof(IdentityPart), builder => builder
                .Attachable()
                .WithDescription("Automatically assigns a unique identity which is stable across systems."));

            SchemaBuilder.CreateMapIndexTable(nameof(IdentityPartIndex), table => table
                .Column<string>("Identifier", col => col.WithLength(36))
                .Column<string>("ContentItemId", c => c.WithLength(32))
                .Column<int>("Latest")
                .Column<int>("Number")
                .Column<int>("Published")
            );

            return 1;
        }
    }
}
