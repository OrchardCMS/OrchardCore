using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
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
            var liquidTemplateManager = serviceProvider.GetService<ILiquidTemplateManager>();
            var htmlEncoder = serviceProvider.GetService<HtmlEncoder>();

            var model = new MarkdownBodyPartViewModel()
            {
                Markdown = ctx.Source.Markdown,
                MarkdownBodyPart = ctx.Source,
                ContentItem = ctx.Source.ContentItem
            };

            var markdown = await liquidTemplateManager.RenderAsync(ctx.Source.Markdown, htmlEncoder, model,
                scope => scope.SetValue("ContentItem", model.ContentItem));

            return Markdig.Markdown.ToHtml(markdown);
        }
    }
}
