using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.ContentLocalization.GraphQL;

public class LocalizationQueryObjectType : ObjectGraphType<LocalizationPart>
{
    public LocalizationQueryObjectType(IStringLocalizer<LocalizationQueryObjectType> S)
    {
        Name = "LocalizationPart";
        Description = S["Localization cultures for your content item."];

        Field(x => x.Culture).Description(S["The culture for your content item."]);
        Field(x => x.LocalizationSet).Description(S["The localization set for your content item."]);

        Field<ListGraphType<ContentItemInterface>, IEnumerable<ContentItem>>("Localizations")
            .Description(S["The localizations of the content item."])
            .Argument<StringGraphType>("culture", "the culture of the content item")
            .ResolveLockedAsync(GetContentItemsByLocalizationSetAsync);
    }

    private static async ValueTask<IEnumerable<ContentItem>> GetContentItemsByLocalizationSetAsync(IResolveFieldContext<LocalizationPart> context)
    {
        var culture = context.GetArgument<string>("culture");
        var contentLocalizationManager = context.RequestServices.GetService<IContentLocalizationManager>();

        if (culture != null)
        {
            var contentItem = await contentLocalizationManager.GetContentItemAsync(context.Source.LocalizationSet, culture);

            return contentItem != null ? new[] { contentItem } : [];
        }

        return await contentLocalizationManager.GetItemsForSetAsync(context.Source.LocalizationSet);
    }
}
