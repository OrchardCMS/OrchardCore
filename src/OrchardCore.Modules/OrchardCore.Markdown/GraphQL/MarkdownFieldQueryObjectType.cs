using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using Fluid.Values;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;
using OrchardCore.Shortcodes.Services;
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
        var markdownService = serviceProvider.GetRequiredService<IMarkdownService>();
        var shortcodeService = serviceProvider.GetRequiredService<IShortcodeService>();

        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();

        var jObject = (JsonObject)ctx.Source.Content;
        // The JObject.Path is consistent here even when contained in a bag part.
        var jsonPath = jObject.GetNormalizedPath();
        var paths = jsonPath.Split('.');
        var partName = paths[0];
        var fieldName = paths[1];
        var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(ctx.Source.ContentItem.ContentType);
        var contentPartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.Name, partName, StringComparison.Ordinal));
        var contentPartFieldDefinition = contentPartDefinition.PartDefinition.Fields.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.Ordinal));

        var settings = contentPartFieldDefinition.GetSettings<MarkdownFieldSettings>();

        // The default Markdown option is to entity escape html
        // so filters must be run after the markdown has been processed.
        var html = markdownService.ToHtml(ctx.Source.Markdown);

        // The liquid rendering is for backwards compatibility and can be removed in a future version.
        if (!settings.SanitizeHtml)
        {
            var liquidTemplateManager = serviceProvider.GetService<ILiquidTemplateManager>();
            var htmlEncoder = serviceProvider.GetService<HtmlEncoder>();

            var model = new MarkdownFieldViewModel()
            {
                Markdown = ctx.Source.Markdown,
                Html = html,
                Field = ctx.Source,
                Part = ctx.Source.ContentItem.Get<ContentPart>(partName),
                PartFieldDefinition = contentPartFieldDefinition
            };

            html = await liquidTemplateManager.RenderStringAsync(html, htmlEncoder, model,
                new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(ctx.Source.ContentItem) });
        }

        html = await shortcodeService.ProcessAsync(html,
            new Context
            {
                ["ContentItem"] = ctx.Source.ContentItem,
                ["PartFieldDefinition"] = contentPartFieldDefinition
            });

        if (settings.SanitizeHtml)
        {
            var htmlSanitizerService = serviceProvider.GetRequiredService<IHtmlSanitizerService>();
            html = htmlSanitizerService.Sanitize(html);
        }

        return html;
    }
}
