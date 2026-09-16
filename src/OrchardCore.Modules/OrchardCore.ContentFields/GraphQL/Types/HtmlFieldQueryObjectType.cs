using System.Text.Json.Nodes;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Html.Services;
using Shortcodes;

namespace OrchardCore.ContentFields.GraphQL;

public class HtmlFieldQueryObjectType : ObjectGraphType<HtmlField>
{
    public HtmlFieldQueryObjectType(IStringLocalizer<HtmlFieldQueryObjectType> S)
    {
        Name = nameof(HtmlField);
        Description = S["Content stored as HTML."];

        Field<StringGraphType>("html")
            .Description(S["the HTML content"])
            .ResolveLockedAsync(RenderHtml);
    }

    private static async ValueTask<object> RenderHtml(IResolveFieldContext<HtmlField> ctx)
    {
        var serviceProvider = ctx.RequestServices;
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
        var settings = contentPartFieldDefinition.GetSettings<HtmlFieldSettings>();

        var model = new DisplayHtmlFieldViewModel
        {
            Html = ctx.Source.Html,
            Field = ctx.Source,
            Part = ctx.Source.ContentItem.Get<ContentPart>(partName),
            PartFieldDefinition = contentPartFieldDefinition,
            ContentItem = ctx.Source.ContentItem,
        };

        var htmlDisplayService = serviceProvider.GetRequiredService<IHtmlDisplayService>();
        await htmlDisplayService.UpdateModelHtmlAsync(
            model,
            new Context
            {
                ["PartFieldDefinition"] = contentPartFieldDefinition,
            },
            settings.SanitizeHtml);

        return model.Html;
    }
}
