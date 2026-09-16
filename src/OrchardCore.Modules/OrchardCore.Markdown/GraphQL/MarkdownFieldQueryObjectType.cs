using System.Text.Json.Nodes;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using Shortcodes;

namespace OrchardCore.Markdown.GraphQL;

public class MarkdownFieldQueryObjectType : ObjectGraphType<MarkdownField>
{
    public MarkdownFieldQueryObjectType(IStringLocalizer<MarkdownFieldQueryObjectType> S)
    {
        Name = nameof(MarkdownField);
        Description = S["Content stored as Markdown. You can also query the HTML interpreted version of Markdown."];

        Field("markdown", x => x.Markdown, nullable: true)
            .Description(S["the markdown value"]);
        Field<StringGraphType>("html")
            .Description(S["the HTML representation of the markdown content"])
            .ResolveLockedAsync(ToHtml);
    }

    private static async ValueTask<object> ToHtml(IResolveFieldContext<MarkdownField> ctx)
    {
        if (string.IsNullOrEmpty(ctx.Source.Markdown))
        {
            return ctx.Source.Markdown;
        }

        var serviceProvider = ctx.RequestServices;
        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();

        var jObject = (JsonObject)ctx.Source.Content;
        // The JObject.Path is consistent here even when contained in a bag part.
        var jsonPath = jObject.GetNormalizedPath();
        var paths = jsonPath.Split('.');
        var partName = paths[0];
        var fieldName = paths[1];
        var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(ctx.Source.ContentItem.ContentType);
        var contentPartDefinition = contentTypeDefinition.Parts
            .FirstOrDefault(x => string.Equals(x.Name, partName, StringComparison.Ordinal));
        var contentPartFieldDefinition = contentPartDefinition.PartDefinition.Fields
            .FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.Ordinal));

        var settings = contentPartFieldDefinition.GetSettings<MarkdownFieldSettings>();

        var markdownDisplayService = serviceProvider.GetRequiredService<IMarkdownDisplayService>();
        return await markdownDisplayService.ToHtmlAsync(
            ctx.Source.Markdown,
            new Context
            {
                ["ContentItem"] = ctx.Source.ContentItem,
                ["PartFieldDefinition"] = contentPartFieldDefinition,
            },
            settings.SanitizeHtml);
    }
}
