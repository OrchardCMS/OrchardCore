using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Title
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
            _contentDefinitionManager.AlterPartDefinition("TitlePart", builder => builder
                .Attachable()
                .WithDescription("Provides a Title for your content item."));

            return 1;
        }
    }
}