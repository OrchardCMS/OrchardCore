using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Infrastructure.SafeCodeFilters;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Services;

namespace OrchardCore.Markdown.GraphQL
{
    public class MarkdownFieldQueryObjectType : ObjectGraphType<MarkdownField>
    {
        public MarkdownFieldQueryObjectType(IStringLocalizer<MarkdownFieldQueryObjectType> S)
        {
            Name = nameof(MarkdownField);
            Description = S["Content stored as Markdown. You can also query the HTML interpreted version of Markdown."];

            Field("markdown", x => x.Markdown, nullable: true)
                .Description(S["the markdown value"]);

            Field<StringGraphType>()
                .Name("html")
                .Description(S["the HTML representation of the markdown content"])
                .ResolveLockedAsync(ToHtml);
        }

        private static async Task<object> ToHtml(ResolveFieldContext<MarkdownField> ctx)
        {
            if (string.IsNullOrEmpty(ctx.Source.Markdown))
            {
                return ctx.Source.Markdown;
            }

            var serviceProvider = ctx.ResolveServiceProvider();

            var markdownService = serviceProvider.GetRequiredService<IMarkdownService>();
            var safeCodeFilterManager = serviceProvider.GetRequiredService<ISafeCodeFilterManager>();
            //var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();

            //TODO this will not work, because we don not know which part is it on.
            // need more context. Probably there is something in the registration of graphql to give us that.
            //var contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(ctx.Source.ContentItem.ContentType);
            //var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "MarkdownBodyPart"));
            //var settings = contentTypePartDefinition.GetSettings<MarkdownFieldSettings>();

            var markdown = ctx.Source.Markdown;
            var liquidTemplateManager = serviceProvider.GetService<ILiquidTemplateManager>();
            var htmlEncoder = serviceProvider.GetService<HtmlEncoder>();

            markdown = await liquidTemplateManager.RenderAsync(markdown, htmlEncoder);

            markdown = await safeCodeFilterManager.ProcessAsync(markdown);

            markdown = markdownService.ToHtml(markdown);

            //TODO sanitize.

            return markdown;
        }
    }
}
