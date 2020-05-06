using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.PublishLater.Indexes;
using OrchardCore.PublishLater.Models;

namespace OrchardCore.PublishLater
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
            _contentDefinitionManager.AlterPartDefinition(nameof(PublishLaterPart), builder => builder
                .Attachable()
                .WithDescription("Adds the ability to schedule content items to be published at a given future date and time."));

            SchemaBuilder.CreateMapIndexTable(nameof(PublishLaterPartIndex), table => table
                .Column<string>(nameof(PublishLaterPartIndex.ScheduledPublishUtc))
            );

            SchemaBuilder.AlterTable(nameof(PublishLaterPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(PublishLaterPartIndex)}_{nameof(PublishLaterPartIndex.ScheduledPublishUtc)}",
                    nameof(PublishLaterPartIndex.ScheduledPublishUtc))
            );

            return 1;
        }
    }
}
