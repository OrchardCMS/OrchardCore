using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Models;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasInputObjectType : WhereInputObjectGraphType<AliasPart>
    {
        public AliasInputObjectType(IStringLocalizer<AliasInputObjectType> S)
        {
            Name = "AliasPartInput";
            Description = S["the alias part of the content item"];

            AddScalarFilterFields<StringGraphType>("alias", S["the alias of the content item"]);
        }
    }
}
