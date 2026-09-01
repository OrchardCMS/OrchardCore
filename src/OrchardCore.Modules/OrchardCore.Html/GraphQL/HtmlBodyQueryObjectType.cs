using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Html.Models;
using OrchardCore.Html.Services;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using Shortcodes;

namespace OrchardCore.Html.GraphQL;

public class HtmlBodyQueryObjectType : ObjectGraphType<HtmlBodyPart>
{
    public HtmlBodyQueryObjectType(IStringLocalizer<HtmlBodyQueryObjectType> S)
    {
        Name = "HtmlBodyPart";
        Description = S["Content stored as HTML."];

        Field<StringGraphType>("html")
            .Description(S["the HTML content"])
            .ResolveLockedAsync(RenderHtml);
    }

    private static async ValueTask<object> RenderHtml(IResolveFieldContext<HtmlBodyPart> ctx)
    {
        var contentDefinitionManager = ctx.RequestServices.GetRequiredService<IContentDefinitionManager>();

        var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(ctx.Source.ContentItem.ContentType);
        var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "HtmlBodyPart", StringComparison.Ordinal));
        var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartSettings>();

        var model = new HtmlBodyPartViewModel
        {
            Html = ctx.Source.Html,
            HtmlBodyPart = ctx.Source,
            ContentItem = ctx.Source.ContentItem,
            TypePartDefinition = contentTypePartDefinition,
        };

        var htmlDisplayService = ctx.RequestServices.GetRequiredService<IHtmlDisplayService>();
        await htmlDisplayService.UpdateModelHtmlAsync(
            model,
            new Context
            {
                ["TypePartDefinition"] = contentTypePartDefinition,
            },
            settings.SanitizeHtml);

        return model.Html;
    }
}
