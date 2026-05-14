using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Settings;

namespace OrchardCore.Markdown;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("MarkdownBodyPart", builder => builder
            .Attachable()
            .WithDescription("Provides a Markdown formatted body for your content item."));

        // Shortcut other migration steps on new content definition schemas.
        return 5;
    }

    // Migrate FieldSettings. This only needs to run on old content definition schemas.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager.MigrateFieldSettingsAsync<MarkdownField, MarkdownFieldSettings>();

        return 4; // Returning 4 instead of 2 to skip the next 2 migration steps, see below why.
    }

    // Previously, Liquid rendering was enabled by not having Html sanitization enabled and UpdateFrom2Async and
    // UpdateFrom3Async disabled sanitization to ensure that MarkdownBodyParts and MarkdownFields kept Liquid rendering
    // enabled. Since Liquid rendering is now controlled by a separate setting, disabling sanitization is no longer
    // necessary.

    public async Task<int> UpdateFrom4Async()
    {
        // To keep the same behavior as before, RenderLiquid is initialized to the opposite of SanitizeHtml.
        foreach (var contentType in await _contentDefinitionManager.LoadTypeDefinitionsAsync())
        {
            if (contentType.Parts.Any(x => x.PartDefinition.Name == "MarkdownBodyPart"))
            {
                await _contentDefinitionManager.AlterTypeDefinitionAsync(contentType.Name, x => x.WithPart("MarkdownBodyPart", part =>
                {
                    part.MergeSettings<MarkdownBodyPartSettings>(s => s.RenderLiquid = !s.SanitizeHtml);
                }));
            }
        }

        var partDefinitions = await _contentDefinitionManager.LoadPartDefinitionsAsync();
        foreach (var partDefinition in partDefinitions)
        {
            if (partDefinition.Fields.Any(x => x.FieldDefinition.Name == "MarkdownField"))
            {
                await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
                {
                    foreach (var fieldDefinition in partDefinition.Fields.Where(x => x.FieldDefinition.Name == "MarkdownField"))
                    {
                        partBuilder.WithField(fieldDefinition.Name, fieldBuilder =>
                        {
                            fieldBuilder.MergeSettings<MarkdownFieldSettings>(s => s.RenderLiquid = !s.SanitizeHtml);
                        });
                    }
                });
            }
        }

        return 5;
    }
}
