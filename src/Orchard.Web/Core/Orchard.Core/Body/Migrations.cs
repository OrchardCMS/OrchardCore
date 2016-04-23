using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Core.Body
{
    public class Migrations : DataMigrations
    {
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("BodyPart", builder => builder
                .Attachable()
                .WithDescription("Provides a Body for your content item."));

            return 1;
        }
    }
}