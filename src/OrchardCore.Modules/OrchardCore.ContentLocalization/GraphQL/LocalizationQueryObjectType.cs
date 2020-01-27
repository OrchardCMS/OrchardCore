using System.Collections.Generic;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.ContentLocalization.GraphQL
{
    public class LocalizationQueryObjectType : ObjectGraphType<LocalizationPart>
    {
        public LocalizationQueryObjectType(IStringLocalizer<LocalizationQueryObjectType> S)
        {
            Name = "LocalizationPart";
            Description = S["Localization cultures for your content item."];

            Field(x => x.Culture).Description(S["The culture for your content item."]);
            Field(x => x.LocalizationSet).Description(S["The localization set for your content item."]);

            Field<ListGraphType<ContentItemType>, IEnumerable<ContentItem>>()
                .Name("Localizations")
                .Description(S["The localizations of the content item."])
                .Argument<StringGraphType, string>("culture", "the culture of the content item", null)
                .ResolveAsync( async x =>
                {
                    var culture = x.GetArgument<string>("culture");
                    var context = (GraphQLContext)x.UserContext;
                    var contentLocalizationManager = context.ServiceProvider.GetService<IContentLocalizationManager>();

                    if (culture != null)
                    {
                        return new List<ContentItem>
                        {
                            await contentLocalizationManager.GetContentItemAsync(x.Source.LocalizationSet, culture)
                        };
                    }

                    return await contentLocalizationManager.GetItemsForSetAsync(x.Source.LocalizationSet);
                });
        }
    }
}
