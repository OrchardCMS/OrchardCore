using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using Shortcodes;

namespace OrchardCore.Markdown.GraphQL;

public class MarkdownBodyQueryObjectType : ObjectGraphType<MarkdownBodyPart>
{
    public MarkdownBodyQueryObjectType(IStringLocalizer<MarkdownBodyQueryObjectType> S)
    {
        Name = nameof(MarkdownBodyPart);
        Description = S["Content stored as Markdown. You can also query the HTML interpreted version of Markdown."];

        Field("markdown", x => x.Markdown, nullable: true)
            .Description(S["the markdown value"]);
        Field<StringGraphType>("html")
            .Description(S["the HTML representation of the markdown content"])
            .ResolveLockedAsync(ToHtml);
    }

    private static async ValueTask<object> ToHtml(IResolveFieldContext<MarkdownBodyPart> ctx)
    {
        if (string.IsNullOrEmpty(ctx.Source.Markdown))
        {
            return ctx.Source.Markdown;
        }

        var serviceProvider = ctx.RequestServices;
        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();

        var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(ctx.Source.ContentItem.ContentType);
        var contentTypePartDefinition = contentTypeDefinition.Parts
            .FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "MarkdownBodyPart", StringComparison.Ordinal));
        var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

        var markdownDisplayService = serviceProvider.GetRequiredService<IMarkdownDisplayService>();
        return await markdownDisplayService.ToHtmlAsync(
            ctx.Source.Markdown,
            new Context
            {
                ["ContentItem"] = ctx.Source.ContentItem,
                ["TypePartDefinition"] = contentTypePartDefinition,
            },
            settings.SanitizeHtml);
    }
}
