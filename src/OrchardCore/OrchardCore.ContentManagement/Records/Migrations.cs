using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.ContentManagement.Records;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<ContentItemIndex>(table => table
            .Column<string>("ContentItemId", c => c.WithLength(26))
            .Column<string>("ContentItemVersionId", c => c.WithLength(26))
            .Column<bool>("Latest")
            .Column<bool>("Published")
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<DateTime>("ModifiedUtc", column => column.Nullable())
            .Column<DateTime>("PublishedUtc", column => column.Nullable())
            .Column<DateTime>("CreatedUtc", column => column.Nullable())
            .Column<string>("Owner", column => column.Nullable().WithLength(ContentItemIndex.MaxOwnerSize))
            .Column<string>("Author", column => column.Nullable().WithLength(ContentItemIndex.MaxAuthorSize))
            .Column<string>("DisplayText", column => column.Nullable().WithLength(ContentItemIndex.MaxDisplayTextSize))
        );

        // YesSql already indexes DocumentId for the joins (IDX_FK_ContentItemIndex), so this one is here for the columns it covers:
        // a list that matches most of the table, such as the default admin list, reads its sort date here instead of in the table.
        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .CreateIndex("IDX_ContentItemIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType",
                "CreatedUtc",
                "ModifiedUtc",
                "PublishedUtc",
                "Published",
                "Latest")
        );

        await CreateContentItemIdIndexAsync();
        await CreateFilterIndexesAsync();

        // Shortcut other migration steps on new content definition schemas.
        return 8;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .AddColumn<string>("ContentItemVersionId", c => c.WithLength(26))
        );

        return 2;
    }

    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .AddColumn<string>("DisplayText", column => column.Nullable().WithLength(ContentItemIndex.MaxDisplayTextSize))
        );

        return 3;
    }

    // Migrate content type definitions. This only needs to run on old content definition schemas.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        var contentTypeDefinitions = await _contentDefinitionManager.LoadTypeDefinitionsAsync();
        foreach (var contentTypeDefinition in contentTypeDefinitions)
        {
            var existingContentTypeSettings = contentTypeDefinition.Settings.ToObject<ContentTypeSettings>();

            // Do this before creating builder, so settings are removed from the builder settings object.
            // Remove existing properties from JObject
            var contentTypeSettingsProperties = existingContentTypeSettings.GetType().GetProperties();
            foreach (var property in contentTypeSettingsProperties)
            {
                contentTypeDefinition.Settings.Remove(property.Name);
            }

            await _contentDefinitionManager.AlterTypeDefinitionAsync(contentTypeDefinition.Name, builder =>
            {
                builder.WithSettings(existingContentTypeSettings);

                foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
                {
                    var existingTypePartSettings = contentTypePartDefinition.Settings.ToObject<ContentTypePartSettings>();

                    // Remove existing properties from JObject
                    var contentTypePartSettingsProperties = existingTypePartSettings.GetType().GetProperties();
                    foreach (var property in contentTypePartSettingsProperties)
                    {
                        contentTypePartDefinition.Settings.Remove(property.Name);
                    }

                    builder.WithPart(contentTypePartDefinition.Name, contentTypePartDefinition.PartDefinition, partBuilder =>
                    {
                        partBuilder.WithSettings(existingTypePartSettings);
                    });
                }
            });
        }

        return 4;
    }

    // Migration content part definitions.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom4Async()
    {
        var partDefinitions = await _contentDefinitionManager.LoadPartDefinitionsAsync();
        foreach (var partDefinition in partDefinitions)
        {
            var existingPartSettings = partDefinition.Settings.ToObject<ContentPartSettings>();

            // Do this before creating builder, so settings are removed from the builder settings object.
            // Remove existing properties from JObject
            var contentTypeSettingsProperties = existingPartSettings.GetType().GetProperties();
            foreach (var property in contentTypeSettingsProperties)
            {
                partDefinition.Settings.Remove(property.Name);
            }

            await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
            {
                partBuilder.WithSettings(existingPartSettings);
                foreach (var fieldDefinition in partDefinition.Fields)
                {
                    var existingFieldSettings = fieldDefinition.Settings.ToObject<ContentPartFieldSettings>();

                    // Do this before creating builder, so settings are removed from the builder settings object.
                    // Remove existing properties from JObject
                    var fieldSettingsProperties = existingFieldSettings.GetType().GetProperties();
                    foreach (var property in fieldSettingsProperties)
                    {
                        fieldDefinition.Settings.Remove(property.Name);
                    }

                    partBuilder.WithField(fieldDefinition.Name, fieldBuilder =>
                    {
                        fieldBuilder.WithSettings(existingFieldSettings);
                    });
                }
            });
        }

        return 5;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom5Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .CreateIndex("IDX_ContentItemIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .CreateIndex("IDX_ContentItemIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType",
                "CreatedUtc",
                "ModifiedUtc",
                "PublishedUtc",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .CreateIndex("IDX_ContentItemIndex_DocumentId_Owner",
                "DocumentId",
                "Owner",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .CreateIndex("IDX_ContentItemIndex_DocumentId_Author",
                "DocumentId",
                "Author",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .CreateIndex("IDX_ContentItemIndex_DocumentId_DisplayText",
                "DocumentId",
                "DisplayText",
                "Published",
                "Latest")
        );

        return 6;
    }

    public async Task<int> UpdateFrom6Async()
    {
        await CreateContentItemIdIndexAsync();

        return 7;
    }

    public async Task<int> UpdateFrom7Async()
    {
        await CreateFilterIndexesAsync();

        // A join on DocumentId seeks on IDX_FK_ContentItemIndex or IDX_ContentItemIndex_DocumentId_ContentType. All these added is a
        // smaller index to scan for a filter that can't seek anyway, such as on DisplayText with Contains or on Author, which isn't
        // worth their cost on every write. A site that relies on such a filter can add the index it needs.
        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table =>
        {
            table.DropIndex("IDX_ContentItemIndex_DocumentId");
            table.DropIndex("IDX_ContentItemIndex_DocumentId_Owner");
            table.DropIndex("IDX_ContentItemIndex_DocumentId_Author");
            table.DropIndex("IDX_ContentItemIndex_DocumentId_DisplayText");
        });

        return 8;
    }

    // No other index leads with ContentItemId, so a query by ContentItemId (IContentManager.GetAsync, for one item or a batch) can
    // only seek on this one. DocumentId is the join to Document; Published and Latest are what a read by ID filters on.
    private Task CreateContentItemIdIndexAsync()
        => SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .CreateIndex("IDX_ContentItemIndex_ContentItemId",
                "ContentItemId",
                "DocumentId",
                "Published",
                "Latest")
        );

    // A filter on one of these columns can't seek on an index that leads with DocumentId, so it scans an index or the table however
    // few items it matches. On ContentType and Owner: the contents admin list and its count, GraphQL, pickers, sitemaps. On
    // ContentItemVersionId: IContentManager.GetVersionAsync and loading the results of a Lucene or Elasticsearch query.
    //
    // Published and Latest come after DocumentId, read from the index without being part of the seek, for two reasons. One index
    // then serves both of them, on a type or an owner. And with Latest before DocumentId, MySQL's estimates change for the default
    // admin list, which matches almost every type: it starts from the Type index of Document instead, which takes twice as long.
    // A read by version filters on neither.
    //
    // No date is added for sorting, as a paged list reads every match whatever the index: YesSql groups by document before it
    // sorts. Holding the dates after these columns makes the first page of one type faster, as it doesn't read the table, but
    // MySQL and SQLite then scan the wider index for the default admin list, which takes up to 20% longer. Author and DisplayText
    // are left out: only the optional GraphQL where filters on Author, and DisplayText is filtered with Contains, which can't seek.
    private async Task CreateFilterIndexesAsync()
    {
        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .CreateIndex("IDX_ContentItemIndex_ContentType",
                "ContentType",
                "DocumentId",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .CreateIndex("IDX_ContentItemIndex_Owner",
                "Owner",
                "DocumentId",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentItemIndex>(table => table
            .CreateIndex("IDX_ContentItemIndex_ContentItemVersionId",
                "ContentItemVersionId",
                "DocumentId")
        );
    }
}
