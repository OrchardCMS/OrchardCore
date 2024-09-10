using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Settings;
using YesSql.Sql;

namespace OrchardCore.Taxonomies;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Taxonomy", taxonomy => taxonomy
            .Draftable()
            .Versionable()
            .Creatable()
            .Listable()
            .WithPart("TitlePart", part => part.WithPosition("1"))
            .WithPart("AliasPart", part => part
                .WithPosition("2")
                .WithSettings(new AliasPartSettings
                {
                    Pattern = "{{ Model.ContentItem | display_text | slugify }}"
                }))
            .WithPart("AutoroutePart", part => part
                .WithPosition("3")
                .WithSettings(new AutoroutePartSettings
                {
                    Pattern = "{{ Model.ContentItem | display_text | slugify }}",
                    AllowRouteContainedItems = true
                }))
            .WithPart("TaxonomyPart", part => part.WithPosition("4"))
        );

        await SchemaBuilder.CreateMapIndexTableAsync<TaxonomyIndex>(table => table
            .Column<string>("TaxonomyContentItemId", c => c.WithLength(26))
            .Column<string>("ContentItemId", c => c.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<string>("TermContentItemId", column => column.WithLength(26))
            .Column<bool>("Published", c => c.WithDefault(true))
            .Column<bool>("Latest", c => c.WithDefault(false))
        );

        await SchemaBuilder.AlterIndexTableAsync<TaxonomyIndex>(table => table
            .CreateIndex("IDX_TaxonomyIndex_DocumentId",
                "DocumentId",
                "TaxonomyContentItemId",
                "ContentItemId",
                "TermContentItemId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<TaxonomyIndex>(table => table
            .CreateIndex("IDX_TaxonomyIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        // Shortcut other migration steps on new content definition schemas.
        return 5;
    }

    // Migrate FieldSettings. This only needs to run on old content definition schemas.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager.MigrateFieldSettingsAsync<TaxonomyField, TaxonomyFieldSettings>();
        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Taxonomy", taxonomy => taxonomy
            .WithPart("AutoroutePart", part => part
                .WithPosition("3")
                .WithSettings(new AutoroutePartSettings
                {
                    Pattern = "{{ Model.ContentItem | display_text | slugify }}",
                    AllowRouteContainedItems = true
                }))
            .WithPart("TaxonomyPart", part => part.WithPosition("4"))
        );

        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        // This step has been updated to also add these new columns.
        await SchemaBuilder.AlterIndexTableAsync<TaxonomyIndex>(table => table
            .AddColumn<bool>("Published", c => c.WithDefault(true))
        );

        await SchemaBuilder.AlterIndexTableAsync<TaxonomyIndex>(table => table
            .AddColumn<bool>("Latest", c => c.WithDefault(false))
        );

        // So that the new indexes can be fully created.
        await SchemaBuilder.AlterIndexTableAsync<TaxonomyIndex>(table => table
            .CreateIndex("IDX_TaxonomyIndex_DocumentId",
                "DocumentId",
                "TaxonomyContentItemId",
                "ContentItemId",
                "TermContentItemId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<TaxonomyIndex>(table => table
            .CreateIndex("IDX_TaxonomyIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        // We then shortcut the next migration step.
        return 5;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom4Async()
    {
        // This step run only if the previous one was executed before
        // it was updated, so here we also add the following columns.
        await SchemaBuilder.AlterIndexTableAsync<TaxonomyIndex>(table => table
            .AddColumn<bool>("Published", c => c.WithDefault(true))
        );

        await SchemaBuilder.AlterIndexTableAsync<TaxonomyIndex>(table => table
            .AddColumn<bool>("Latest", c => c.WithDefault(false))
        );

        // But we create a separate index for these new columns.
        await SchemaBuilder.AlterIndexTableAsync<TaxonomyIndex>(table => table
            .CreateIndex("IDX_TaxonomyIndex_DocumentId_Published",
                "DocumentId",
                "Published",
                "Latest")
        );

        return 5;
    }
}

internal sealed class AliasPartSettings
{
    public string Pattern { get; set; }
}

internal sealed class AutoroutePartSettings
{
    public string Pattern { get; set; }
    public bool AllowRouteContainedItems { get; set; }
}
