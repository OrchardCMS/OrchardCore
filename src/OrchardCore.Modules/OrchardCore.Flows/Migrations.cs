using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows
{
    public class Migrations : DataMigration
    {
        private IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("FlowPart", builder => builder
                .Attachable()
                .WithDescription("Provides a customizable body for your content item."));

            return 1;
        }

        public int UpdateFrom1()
        {
            _contentDefinitionManager.AlterPartDefinition("BagPart", builder => builder
                .Attachable()
                .Reusable()
                .WithDescription("Provides a collection behavior for your content item."));

            // Return 3 to shortcut the third migration on new content definition schemas.
            return 3;
        }

        // Migrate PartSettings. This only needs to run on old content definition schemas.
        // This code can be removed in a later version.
        public int UpdateFrom2()
        {
            _contentDefinitionManager.MigratePartSettings<BagPart, BagPartSettings>();

            return 3;
        }
    }
}
