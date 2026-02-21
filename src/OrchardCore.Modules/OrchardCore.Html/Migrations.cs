using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using OrchardCore.Html.Fields;
using OrchardCore.Html.Indexing;
using OrchardCore.Html.Settings;
using YesSql.Sql;

namespace OrchardCore.Html;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("HtmlBodyPart", builder => builder
            .Attachable()
            .WithDescription("Provides an HTML Body for your content item."));

        // Html field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<HtmlField, HtmlFieldSettings>();

        // For backwards compatibility with liquid filters we disable html sanitization on existing field definitions.
        var partDefinitions = await _contentDefinitionManager.LoadPartDefinitionsAsync();

        foreach (var partDefinition in partDefinitions)
        {
            if (partDefinition.Fields.Any(x => x.FieldDefinition.Name == "HtmlField"))
            {
                await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
                {
                    foreach (var fieldDefinition in partDefinition.Fields.Where(x => x.FieldDefinition.Name == "HtmlField"))
                    {
                        partBuilder.WithField(fieldDefinition.Name, fieldBuilder =>
                        {
                            fieldBuilder.MergeSettings<HtmlFieldSettings>(x => x.SanitizeHtml = false);
                        });
                    }
                });
            }
        }

        await SchemaBuilder.CreateMapIndexTableAsync<HtmlFieldIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ContentItemVersionId", column => column.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<bool>("Published", column => column.Nullable())
            .Column<bool>("Latest", column => column.Nullable())
            .Column<string>("Html", column => column.Nullable().Unlimited())
        );

        await SchemaBuilder.AlterIndexTableAsync<HtmlFieldIndex>(table => table
            .CreateIndex("IDX_HtmlFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<HtmlFieldIndex>(table => table
            .CreateIndex("IDX_HtmlFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        return 1;
    }
}
