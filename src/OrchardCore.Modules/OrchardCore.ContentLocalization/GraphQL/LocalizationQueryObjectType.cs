using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.Models;

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
        }
    }
}
