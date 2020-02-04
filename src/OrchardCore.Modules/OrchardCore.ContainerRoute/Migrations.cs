using OrchardCore.ContainerRoute.Drivers;
using OrchardCore.ContainerRoute.Indexes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.ContainerRoute
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
            _contentDefinitionManager.AlterPartDefinition("ContainerRoutePart", builder => builder
                .Attachable()
                .WithDescription("Provides a custom url for your content item and any content items contained within it."));

            SchemaBuilder.CreateMapIndexTable(nameof(ContainerRoutePartIndex), table => table
                .Column<string>("ContainerContentItemId", c => c.WithLength(26))
                .Column<string>("ContainedContentItemId", c => c.WithLength(26))
                .Column<string>("JsonPath", c => c.Unlimited())
                .Column<string>("Path", col => col.WithLength(ContainerRoutePartDisplay.MaxPathLength))
                .Column<bool>("Published")
                .Column<bool>("Latest")
            );

            SchemaBuilder.AlterTable(nameof(ContainerRoutePartIndex), table => table
                .CreateIndex("IDX_ContainerRoutePartIndex_ContentItemIds", "ContainerContentItemId", "ContainedContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(ContainerRoutePartIndex), table => table
                .CreateIndex("IDX_ContainerRoutePartIndex_State", "Published", "Latest")
            );

            return 1;
        }

        public int UpdateFrom1()
        {
            _contentDefinitionManager.AlterPartDefinition("RouteHandlerPart", builder => builder
                .Attachable()
                .WithDescription("Provides a custom url for your contained content item."));

            return 2;
        }
    }
}
