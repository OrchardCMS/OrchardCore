using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Html.Models;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.ShortCodes.Services;
using OrchardCore.Liquid;

namespace OrchardCore.Html.GraphQL
{
    public class HtmlBodyQueryObjectType : ObjectGraphType<HtmlBodyPart>
    {
        public HtmlBodyQueryObjectType(IStringLocalizer<HtmlBodyQueryObjectType> S)
        {
            Name = "HtmlBodyPart";
            Description = S["Content stored as HTML."];

            Field<StringGraphType>()
                .Name("html")
                .Description(S["the HTML content"])
                .ResolveLockedAsync(RenderHtml);
        }

        private static async Task<object> RenderHtml(ResolveFieldContext<HtmlBodyPart> ctx)
        {
            var serviceProvider = ctx.ResolveServiceProvider();
            var shortCodeService = serviceProvider.GetRequiredService<IShortCodeService>();
            var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();

            var contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(ctx.Source.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "HtmlBodyPart"));
            var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartSettings>();

            var html = ctx.Source.Html;
            if (!settings.SanitizeHtml)
            {
                var model = new HtmlBodyPartViewModel()
                {
                    Html = ctx.Source.Html,
                    HtmlBodyPart = ctx.Source,
                    ContentItem = ctx.Source.ContentItem
                };

                var liquidTemplateManager = serviceProvider.GetRequiredService<ILiquidTemplateManager>();
                var htmlEncoder = serviceProvider.GetService<HtmlEncoder>();

                html = await liquidTemplateManager.RenderAsync(html, htmlEncoder, model,
                    scope => scope.SetValue("ContentItem", model.ContentItem));
            }

            return await shortCodeService.ProcessAsync(html);
        }
    }
}
