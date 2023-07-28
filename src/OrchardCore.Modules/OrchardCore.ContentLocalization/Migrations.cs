using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.ContentLocalization.Records
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
            _contentDefinitionManager.AlterPartDefinition(nameof(LocalizationPart), builder => builder
                .Attachable()
                .WithDescription("Provides a way to create localized version of content."));

            SchemaBuilder.CreateMapIndexTable<LocalizedContentItemIndex>(table => table
                .Column<string>("LocalizationSet", col => col.WithLength(26))
                .Column<string>("Culture", col => col.WithLength(16))
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<bool>("Published")
                .Column<bool>("Latest")
            );

            SchemaBuilder.AlterIndexTable<LocalizedContentItemIndex>(table => table
                .CreateIndex("IDX_LocalizationPartIndex_DocumentId",
                "DocumentId",
                "LocalizationSet",
                "Culture",
                "ContentItemId",
                "Published",
                "Latest")
            );

            // Shortcut other migration steps on new content definition schemas.
            return 4;
        }

        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTable<LocalizedContentItemIndex>(table => table
                .AddColumn<bool>(nameof(LocalizedContentItemIndex.Published)));

            return 2;
        }

        // This code can be removed in a later version.
        public int UpdateFrom2()
        {
            SchemaBuilder.AlterIndexTable<LocalizedContentItemIndex>(table => table
                .AddColumn<bool>(nameof(LocalizedContentItemIndex.Latest))
            );

            SchemaBuilder.AlterIndexTable<LocalizedContentItemIndex>(table => table
                .CreateIndex("IDX_LocalizationPartIndex_DocumentId",
                "DocumentId",
                "LocalizationSet",
                "Culture",
                "ContentItemId",
                "Published",
                "Latest")
            );

            return 3;
        }

        // Migrate null LocalizedContentItemIndex Latest column.
#pragma warning disable CA1822 // Mark members as static
        public int UpdateFrom3()
#pragma warning restore CA1822 // Mark members as static
        {
            // Defer this until after the subsequent migrations have succeded as the schema has changed.
            ShellScope.AddDeferredTask(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var localizedContentItems = await session.Query<ContentItem, LocalizedContentItemIndex>().ListAsync();

                foreach (var localizedContentItem in localizedContentItems)
                {
                    localizedContentItem.Latest = localizedContentItem.ContentItem.Latest;
                    session.Save(localizedContentItem);
                }
            });

            return 4;
        }
    }
}
