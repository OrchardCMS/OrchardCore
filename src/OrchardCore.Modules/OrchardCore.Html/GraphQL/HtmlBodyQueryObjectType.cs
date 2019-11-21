using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
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
        public HtmlBodyQueryObjectType(IStringLocalizer<HtmlBodyQueryObjectType> T)
        {
            Name = "HtmlBodyPart";
            Description = T["Content stored as HTML."];

            Field<StringGraphType>()
                .Name("html")
                .Description(T["the HTML content"])
                .ResolveAsync(RenderHtml);
        }

        private static async Task<object> RenderHtml(ResolveFieldContext<HtmlBodyPart> ctx)
        {
            var context = (GraphQLContext)ctx.UserContext;
            var liquidTemplateManager = context.ServiceProvider.GetService<ILiquidTemplateManager>();
            var htmlEncoder = context.ServiceProvider.GetService<HtmlEncoder>();

            var model = new HtmlBodyPartViewModel()
            {
                HtmlBodyPart = ctx.Source,
                ContentItem = ctx.Source.ContentItem
            };

            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", ctx.Source.ContentItem);
            templateContext.MemberAccessStrategy.Register<HtmlBodyPartViewModel>();
            templateContext.SetValue("Model", model);

            return await liquidTemplateManager.RenderAsync(ctx.Source.Html, htmlEncoder, templateContext);
        }
    }
}
