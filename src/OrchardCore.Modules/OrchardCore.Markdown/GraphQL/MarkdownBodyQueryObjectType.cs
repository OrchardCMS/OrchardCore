using System.Text.Encodings.Web;
using Fluid.Values;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;
using OrchardCore.Shortcodes.Services;
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
        var markdownService = serviceProvider.GetRequiredService<IMarkdownService>();
        var shortcodeService = serviceProvider.GetRequiredService<IShortcodeService>();
        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();

        var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(ctx.Source.ContentItem.ContentType);
        var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "MarkdownBodyPart", StringComparison.Ordinal));
        var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

        // The default Markdown option is to entity escape html
        // so filters must be run after the markdown has been processed.
        var html = markdownService.ToHtml(ctx.Source.Markdown);

        // The liquid rendering is for backwards compatibility and can be removed in a future version.
        if (!settings.SanitizeHtml)
        {
            var liquidTemplateManager = serviceProvider.GetService<ILiquidTemplateManager>();
            var htmlEncoder = serviceProvider.GetService<HtmlEncoder>();
            var model = new MarkdownBodyPartViewModel()
            {
                Markdown = ctx.Source.Markdown,
                Html = html,
                MarkdownBodyPart = ctx.Source,
                ContentItem = ctx.Source.ContentItem
            };

            html = await liquidTemplateManager.RenderStringAsync(html, htmlEncoder, model,
                new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });
        }

        html = await shortcodeService.ProcessAsync(html,
            new Context
            {
                ["ContentItem"] = ctx.Source.ContentItem,
                ["PartFieldDefinition"] = contentTypePartDefinition
            });

        if (settings.SanitizeHtml)
        {
            var htmlSanitizerService = serviceProvider.GetRequiredService<IHtmlSanitizerService>();
            html = htmlSanitizerService.Sanitize(html);
        }

        return html;
    }
}
