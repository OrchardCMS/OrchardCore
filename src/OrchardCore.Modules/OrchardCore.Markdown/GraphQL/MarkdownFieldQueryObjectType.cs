using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GraphQL.Builders;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Infrastructure.SafeCodeFilters;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;

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

            var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();

            var jObject = ctx.Source.Content as JObject;
            // The JObject.Path is consistent here even when contained in a bag part.
            var jsonPath = jObject.Path;
            var paths = jsonPath.Split('.');
            var partName = paths[0];
            var fieldName = paths[1];
            var contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(ctx.Source.ContentItem.ContentType);
            var contentPartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.Name, partName));
            var contentPartFieldDefintion = contentPartDefinition.PartDefinition.Fields.FirstOrDefault(x => string.Equals(x.Name, fieldName));

            var settings = contentPartFieldDefintion.GetSettings<MarkdownFieldSettings>();

            var markdown = ctx.Source.Markdown;

            if (settings.AllowCustomScripts)
            {
                var liquidTemplateManager = serviceProvider.GetService<ILiquidTemplateManager>();
                var htmlEncoder = serviceProvider.GetService<HtmlEncoder>();

                var model = new MarkdownFieldViewModel()
                {
                    Markdown = ctx.Source.Markdown,
                    Field = ctx.Source,
                    Part = ctx.Source.ContentItem.Get<ContentPart>(partName),
                    PartFieldDefinition = contentPartFieldDefintion
                };

                markdown = await liquidTemplateManager.RenderAsync(markdown, htmlEncoder, model,
                    scope => scope.SetValue("ContentItem", ctx.Source.ContentItem));
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
