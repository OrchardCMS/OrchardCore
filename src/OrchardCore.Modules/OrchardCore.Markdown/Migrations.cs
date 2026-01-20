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
        return 4;
    }

    // Migrate FieldSettings. This only needs to run on old content definition schemas.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager.MigrateFieldSettingsAsync<MarkdownField, MarkdownFieldSettings>();

        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        // For backwards compatibility with liquid filters we disable html sanitization on existing field definitions.
        foreach (var contentType in await _contentDefinitionManager.LoadTypeDefinitionsAsync())
        {
            if (contentType.Parts.Any(x => x.PartDefinition.Name == "MarkdownBodyPart"))
            {
                await _contentDefinitionManager.AlterTypeDefinitionAsync(contentType.Name, x => x.WithPart("MarkdownBodyPart", part =>
                {
                    part.MergeSettings<MarkdownBodyPartSettings>(x => x.SanitizeHtml = false);
                }));
            }
        }

        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        // For backwards compatibility with liquid filters we disable html sanitization on existing field definitions.
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
                            fieldBuilder.MergeSettings<MarkdownFieldSettings>(s => s.SanitizeHtml = false);
                        });
                    }
                });
            }
        }

        return 4;
    }
}
