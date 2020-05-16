using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Infrastructure.SafeCodeFilters;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.GraphQL
{
    public class MarkdownBodyQueryObjectType : ObjectGraphType<MarkdownBodyPart>
    {
        public MarkdownBodyQueryObjectType(IStringLocalizer<MarkdownBodyQueryObjectType> S)
        {
            Name = nameof(MarkdownBodyPart);
            Description = S["Content stored as Markdown. You can also query the HTML interpreted version of Markdown."];

            Field("markdown", x => x.Markdown, nullable: true)
                .Description(S["the markdown value"]);

            Field<StringGraphType>()
                .Name("html")
                .Description(S["the HTML representation of the markdown content"])
                .ResolveLockedAsync(ToHtml);
        }

        private static async Task<object> ToHtml(ResolveFieldContext<MarkdownBodyPart> ctx)
        {
            if (string.IsNullOrEmpty(ctx.Source.Markdown))
            {
                return ctx.Source.Markdown;
            }

            var serviceProvider = ctx.ResolveServiceProvider();
            var markdownService = serviceProvider.GetRequiredService<IMarkdownService>();
            var safeCodeFilterManager = serviceProvider.GetRequiredService<ISafeCodeFilterManager>();
            var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();

            var contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(ctx.Source.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "MarkdownBodyPart"));
            var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

            var markdown = ctx.Source.Markdown;
            if (settings.AllowCustomScripts)
            {
                var liquidTemplateManager = serviceProvider.GetService<ILiquidTemplateManager>();
                var htmlEncoder = serviceProvider.GetService<HtmlEncoder>();

                var model = new MarkdownBodyPartViewModel()
                {
                    Markdown = ctx.Source.Markdown,
                    MarkdownBodyPart = ctx.Source,
                    ContentItem = ctx.Source.ContentItem
                };

                markdown = await liquidTemplateManager.RenderAsync(ctx.Source.Markdown, htmlEncoder, model,
                    scope => scope.SetValue("ContentItem", model.ContentItem));
            }

            markdown = await safeCodeFilterManager.ProcessAsync(markdown);
            markdown = markdownService.ToHtml(markdown);

            if (!settings.AllowCustomScripts)
            {
                var htmlSanitizerService = serviceProvider.GetRequiredService<IHtmlSanitizerService>();
                markdown = htmlSanitizerService.Sanitize(markdown);
            }

            return markdown;
        }
    }
}
