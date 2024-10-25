using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using OrchardCore.Html.Settings;
using YesSql;

namespace OrchardCore.Html;

public sealed class Migrations : DataMigration
{
    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(
        IContentDefinitionManager contentDefinitionManager,
        ISession session,
        ILogger<Migrations> logger)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _session = session;
        _logger = logger;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("HtmlBodyPart", builder => builder
            .Attachable()
            .WithDescription("Provides an HTML Body for your content item."));

        // Shortcut other migration steps on new content definition schemas.
        return 5;
    }

    // This code can be removed in a later version.
#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom1()
    {
        return 2;
    }

    // This code can be removed in a later version.
    public int UpdateFrom2()
#pragma warning restore CA1822 // Mark members as static
    {
        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        // Update content type definitions
        foreach (var contentType in await _contentDefinitionManager.LoadTypeDefinitionsAsync())
        {
            if (contentType.Parts.Any(x => x.PartDefinition.Name == "BodyPart"))
            {
                await _contentDefinitionManager.AlterTypeDefinitionAsync(contentType.Name, x => x.RemovePart("BodyPart").WithPart("HtmlBodyPart"));
            }
        }

        await _contentDefinitionManager.DeletePartDefinitionAsync("BodyPart");

        // We are patching all content item versions by moving the Title to DisplayText
        // This step doesn't need to be executed for a brand new site

        var lastDocumentId = 0L;

        for (; ; )
        {
            var contentItemVersions = await _session.Query<ContentItem, ContentItemIndex>(x => x.DocumentId > lastDocumentId).Take(10).ListAsync();

            if (!contentItemVersions.Any())
            {
                // No more content item version to process
                break;
            }

            foreach (var contentItemVersion in contentItemVersions)
            {
                if (UpdateBody((JsonObject)contentItemVersion.Content))
                {
                    await _session.SaveAsync(contentItemVersion);
                    _logger.LogInformation("A content item version's BodyPart was upgraded: {ContentItemVersionId}", contentItemVersion.ContentItemVersionId);
                }

                lastDocumentId = contentItemVersion.Id;
            }

            await _session.SaveChangesAsync();
        }

        static bool UpdateBody(JsonNode content)
        {
            var changed = false;

            if (content.GetValueKind() == JsonValueKind.Object)
            {
                var body = content["BodyPart"]?["Body"]?.Value<string>();

                if (!string.IsNullOrWhiteSpace(body))
                {
                    content["HtmlBodyPart"] = new JsonObject() { ["Html"] = body };
                    changed = true;
                }

                foreach (var node in content.AsObject())
                {
                    changed = UpdateBody(node.Value) || changed;
                }
            }

            if (content.GetValueKind() == JsonValueKind.Array)
            {
                foreach (var node in content.AsArray())
                {
                    changed = UpdateBody(node) || changed;
                }
            }

            return changed;
        }

        return 4;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom4Async()
    {
        // For backwards compatibility with liquid filters we disable html sanitization on existing field definitions.
        foreach (var contentType in await _contentDefinitionManager.LoadTypeDefinitionsAsync())
        {
            if (contentType.Parts.Any(x => x.PartDefinition.Name == "HtmlBodyPart"))
            {
                await _contentDefinitionManager.AlterTypeDefinitionAsync(contentType.Name, x => x.WithPart("HtmlBodyPart", part =>
                {
                    part.MergeSettings<HtmlBodyPartSettings>(x => x.SanitizeHtml = false);
                }));
            }
        }

        return 5;
    }
}
