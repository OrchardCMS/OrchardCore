using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Widgets
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
            _contentDefinitionManager.AlterPartDefinition("WidgetsListPart", builder => builder
                .Attachable()
                .WithDescription("Provides a way to add widgets per content items.")
                );

            return 1;
        }
    }
}