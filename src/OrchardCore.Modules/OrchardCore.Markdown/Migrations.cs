using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Settings;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Markdown;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ILogger _logger;

    public Migrations(
        IContentDefinitionManager contentDefinitionManager,
        ILogger<Migrations> logger)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _logger = logger;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("MarkdownBodyPart", builder => builder
            .Attachable()
            .WithDescription("Provides a Markdown formatted body for your content item."));

        // Shortcut other migration steps on new content definition schemas.
        return 6;
    }

    // Migrate FieldSettings. This only needs to run on old content definition schemas.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager.MigrateFieldSettingsAsync<MarkdownField, MarkdownFieldSettings>();

        return 4; // Returning 4 instead of 2 to skip the next 2 migration steps, see below why.
    }

    public async Task<int> UpdateFrom4Async()
    {
        await WarnAboutLiquidTemplatesAsync();

        return 6;
    }

    public async Task<int> UpdateFrom5Async()
    {
        await WarnAboutLiquidTemplatesAsync();

        return 6;
    }

    private async Task WarnAboutLiquidTemplatesAsync()
    {
        foreach (var contentType in await _contentDefinitionManager.LoadTypeDefinitionsAsync())
        {
            foreach (var typePart in contentType.Parts)
            {
                if (string.Equals(typePart.PartDefinition.Name, "MarkdownBodyPart", StringComparison.Ordinal) &&
                    typePart.Settings[nameof(MarkdownBodyPartSettings)]?["RenderLiquid"]?.GetValue<bool>() is true)
                {
                    _logger.LogWarning(
                        "Content type '{ContentType}' part '{Part}' has RenderLiquid enabled. Liquid syntax remains stored but is no longer executed by MarkdownBodyPart. Migrate the authored source manually to LiquidPart.",
                        contentType.Name,
                        typePart.Name);
                }

                foreach (var field in typePart.PartDefinition.Fields.Where(
                    field => string.Equals(field.FieldDefinition.Name, nameof(MarkdownField), StringComparison.Ordinal)))
                {
                    if (field.Settings[nameof(MarkdownFieldSettings)]?["RenderLiquid"]?.GetValue<bool>() is true)
                    {
                        _logger.LogWarning(
                            "Content type '{ContentType}' part '{Part}' field '{Field}' has RenderLiquid enabled. Liquid syntax remains stored but is no longer executed by MarkdownField. Migrate the authored source manually to LiquidField.",
                            contentType.Name,
                            typePart.Name,
                            field.Name);
                    }
                }
            }
        }
    }

}
