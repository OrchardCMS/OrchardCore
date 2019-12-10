using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.GraphQL
{
    public class LocalizationQueryObjectType : ObjectGraphType<LocalizationPart>
    {
        public LocalizationQueryObjectType(IStringLocalizer<LocalizationQueryObjectType> T)
        {
            Name = "LocalizationPart";
            Description = T["Localization cultures for your content item."];

            Field(x => x.Culture).Description(T["The culture for your content item."]);
            Field(x => x.LocalizationSet).Description(T["The localization set for your content item."]);
        }
    }
}
