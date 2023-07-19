using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows
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
            _contentDefinitionManager.AlterPartDefinition("FlowPart", builder => builder
                .Attachable()
                .WithDescription("Provides a customizable body for your content item where you can build a content structure with widgets."));

            _contentDefinitionManager.AlterPartDefinition("BagPart", builder => builder
                .Attachable()
                .Reusable()
                .WithDescription("Provides a collection behavior for your content item where you can place other content items."));

            // Shortcut other migration steps on new content definition schemas.
            return 3;
        }

        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            _contentDefinitionManager.AlterPartDefinition("BagPart", builder => builder
                .Attachable()
                .Reusable()
                .WithDescription("Provides a collection behavior for your content item where you can place other content items."));

            return 2;
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
