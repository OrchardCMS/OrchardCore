using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.GraphQL
{
    public class LocalizationInputObjectType : WhereInputObjectGraphType<LocalizationPart>
    {
        public LocalizationInputObjectType(IStringLocalizer<LocalizationInputObjectType> T)
        {
            Name = "LocalizationInputObjectType";
            Description = T["the localization part of the content item"];

            AddScalarFilterFields<StringGraphType>("culture", T["the culture of the content item to filter"]);
        }
    }
}
