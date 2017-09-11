using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;

namespace OrchardCore.Contents
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
            _contentDefinitionManager.AlterPartDefinition("CommonPart", builder => builder
                .Attachable()
                .WithDescription("Provides an editor for the common properties of a content item."));

            return 1;
        }
    }
}