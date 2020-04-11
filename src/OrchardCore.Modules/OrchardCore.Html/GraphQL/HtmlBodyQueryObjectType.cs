using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Html.Models;
using OrchardCore.Html.ViewModels;
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
            var liquidTemplateManager = serviceProvider.GetService<ILiquidTemplateManager>();
            var htmlEncoder = serviceProvider.GetService<HtmlEncoder>();

            var model = new HtmlBodyPartViewModel()
            {
                Html = ctx.Source.Html,
                HtmlBodyPart = ctx.Source,
                ContentItem = ctx.Source.ContentItem
            };

            return await liquidTemplateManager.RenderAsync(ctx.Source.Html, htmlEncoder, model,
                scope => scope.SetValue("ContentItem", model.ContentItem));
        }
    }
}
