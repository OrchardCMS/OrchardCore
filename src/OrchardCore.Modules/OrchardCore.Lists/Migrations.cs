using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using YesSql.Sql;

namespace OrchardCore.Lists;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("ListPart", builder => builder
            .Attachable()
            .WithDescription("Add a list behavior."));

        await SchemaBuilder.CreateMapIndexTableAsync<ContainedPartIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ListContentItemId", column => column.WithLength(26))
            .Column<string>("DisplayText", column => column.WithLength(ContainedPartIndex.MaxDisplayTextSize))
            .Column<int>("Order")
            .Column<string>("ListContentType")
            .Column<bool>("Published")
            .Column<bool>("Latest")

        );

        await SchemaBuilder.AlterIndexTableAsync<ContainedPartIndex>(table => table
            .CreateIndex("IDX_ContainedPartIndex_DocumentId",
                "Id",
                "DocumentId",
                "ContentItemId",
                "ListContentItemId",
                "DisplayText",
                "Order",
                "ListContentType",
                "Published",
                "Latest")
        );

        // Shortcut other migration steps on new content definition schemas.
        return 4;
    }

    // Migrate PartSettings. This only needs to run on old content definition schemas.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager.MigratePartSettingsAsync<ListPart, ListPartSettings>();

        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<ContainedPartIndex>(table => table
            .CreateIndex("IDX_ContainedPartIndex_DocumentId",
            "DocumentId",
            "ListContentItemId",
            "Order")
        );

        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<ContainedPartIndex>(table => table
            .AddColumn<string>("ContentItemId", column => column.WithLength(26))
        );

        await SchemaBuilder.AlterIndexTableAsync<ContainedPartIndex>(table => table
            .AddColumn<string>("ListContentType")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContainedPartIndex>(table => table
            .AddColumn<string>("DisplayText", column => column.WithLength(ContainedPartIndex.MaxDisplayTextSize))
        );

        await SchemaBuilder.AlterIndexTableAsync<ContainedPartIndex>(table => table
            .AddColumn<bool>("Published")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContainedPartIndex>(table => table
            .AddColumn<bool>("Latest")
        );
        await SchemaBuilder.AlterIndexTableAsync<ContainedPartIndex>(table => table
            .DropIndex("IDX_ContainedPartIndex_DocumentId")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContainedPartIndex>(table => table
            .CreateIndex("IDX_ContainedPartIndex_DocumentId",
                "Id",
                "DocumentId",
                "ContentItemId",
                "ListContentItemId",
                "DisplayText",
                "Order",
                "ListContentType",
                "Published",
                "Latest")
        );

        return 4;
    }
}
