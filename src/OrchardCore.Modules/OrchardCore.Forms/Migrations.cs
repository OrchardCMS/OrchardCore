using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;

namespace OrchardCore.Forms
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
            _contentDefinitionManager.AlterPartDefinition("FormPart", part => part
                .Attachable()
                .WithDescription("Provides form properties to your content item."));

            return 1;
        }
    }
}