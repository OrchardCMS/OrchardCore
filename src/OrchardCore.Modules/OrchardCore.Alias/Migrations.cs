using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.Alias;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(AliasPart), builder => builder
            .Attachable()
            .WithDescription("Provides a way to define custom aliases for content items."));

        await SchemaBuilder.CreateMapIndexTableAsync<AliasPartIndex>(table => table
            .Column<string>("Alias", col => col.WithLength(AliasPart.MaxAliasLength))
            .Column<string>("ContentItemId", c => c.WithLength(26))
            .Column<bool>("Latest", c => c.WithDefault(false))
            .Column<bool>("Published", c => c.WithDefault(true))
        );

        await SchemaBuilder.AlterIndexTableAsync<AliasPartIndex>(table => table
            .CreateIndex("IDX_AliasPartIndex_DocumentId",
                "DocumentId",
                "Alias",
                "ContentItemId",
                "Published",
                "Latest")
        );

        // Shortcut other migration steps on new content definition schemas.
        return 4;
    }

    // This code can be removed in a later version as Latest and Published are alterations.
    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<AliasPartIndex>(table => table
            .AddColumn<bool>("Latest", c => c.WithDefault(false))
        );

        await SchemaBuilder.AlterIndexTableAsync<AliasPartIndex>(table => table
            .AddColumn<bool>("Published", c => c.WithDefault(true))
        );

        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        // Can't be fully created on existing databases where the 'Alias' may be of 767 chars.
        await SchemaBuilder.AlterIndexTableAsync<AliasPartIndex>(table => table
            // .CreateIndex("IDX_AliasPartIndex_DocumentId",
            //    "DocumentId",
            //    "Alias",
            //    "ContentItemId",
            //    "Latest",
            //    "Published")

            .CreateIndex("IDX_AliasPartIndex_DocumentId",
                "DocumentId",
                "Alias")
        );

        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        // In the previous migration step, an index was not fully created,
        // but here, we can create a separate one for the missing columns.
        await SchemaBuilder.AlterIndexTableAsync<AliasPartIndex>(table => table
            .CreateIndex("IDX_AliasPartIndex_DocumentId_ContentItemId",
                "DocumentId",
                "ContentItemId",
                "Published",
                "Latest")
        );

        return 4;
    }
}
