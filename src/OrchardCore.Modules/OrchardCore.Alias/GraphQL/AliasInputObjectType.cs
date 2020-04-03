using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Models;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasInputObjectType : WhereInputObjectGraphType<AliasPart>
    {
        public AliasInputObjectType(IStringLocalizer<AliasInputObjectType> T)
        {
            Name = "AliasPartInput";
            Description = T["the alias part of the content item"];

            AddScalarFilterFields<StringGraphType>("alias", T["the alias of the content item"]);
        }
    }
}
