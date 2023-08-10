using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.ContentPreview
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
            _contentDefinitionManager.AlterPartDefinition("PreviewPart", builder => builder
                .Attachable()
                .WithDescription("Provides a way to define the url that is used to render your content item for preview. You only need to use this for the content preview feature when running the frontend decoupled from the admin."));

            return 1;
        }
    }
}
