using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.PublishLater.Indexes;
using OrchardCore.PublishLater.Models;
using YesSql.Sql;

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

            SchemaBuilder.CreateMapIndexTable<PublishLaterPartIndex>(table => table
                .Column<string>(nameof(PublishLaterPartIndex.ScheduledPublishUtc))
            );

            SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table => table
                .CreateIndex($"IDX_{nameof(PublishLaterPartIndex)}_{nameof(PublishLaterPartIndex.ScheduledPublishUtc)}",
                    "DocumentId",
                    nameof(PublishLaterPartIndex.ScheduledPublishUtc))
            );

            // Shortcut other migration steps on new content definition schemas.
            return 2;
        }

        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table => table
                .CreateIndex($"IDX_{nameof(PublishLaterPartIndex)}_{nameof(PublishLaterPartIndex.ScheduledPublishUtc)}",
                    "DocumentId",
                    nameof(PublishLaterPartIndex.ScheduledPublishUtc))
            );

            return 2;
        }
    }
}
