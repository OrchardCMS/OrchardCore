using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Fields;

namespace OrchardCore.Markdown.GraphQL
{
    public class MarkdownFieldQueryObjectType : ObjectGraphType<MarkdownField>
    {
        public MarkdownFieldQueryObjectType(IStringLocalizer<MarkdownFieldQueryObjectType> T)
        {
            Name = nameof(MarkdownField);
            Description = T["Content stored as Markdown. You can also query the HTML interpreted version of Markdown."];

            Field("markdown", x => x.Markdown, nullable: true)
                .Description(T["the markdown value"])
                .Type(new StringGraphType());

            Field<StringGraphType>()
                .Name("html")
                .Description(T["the HTML representation of the markdown content"])
                .ResolveAsync(ToHtml);
        }

        private static async Task<object> ToHtml(ResolveFieldContext<MarkdownField> ctx)
        {
            var context = (GraphQLContext) ctx.UserContext;
            var liquidTemplateManager = context.ServiceProvider.GetService<ILiquidTemplateManager>();
            var htmlEncoder = context.ServiceProvider.GetService<HtmlEncoder>();

            var markdown = ctx.Source.Markdown;
            var templateContext = new TemplateContext();
            markdown = await liquidTemplateManager.RenderAsync(markdown, htmlEncoder, templateContext);

            return Markdig.Markdown.ToHtml(markdown);
        }
    }
}
