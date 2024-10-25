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

        // Shortcut other migration steps on new content definition schemas.
        return 6;
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
}
