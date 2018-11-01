using GraphQL.Types;
using OrchardCore.Alias.Models;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasInputObjectType : WhereInputObjectGraphType<AliasPart>
    {
        public AliasInputObjectType()
        {
            Name = "AliasPartInput";
            Description = "the alias part of the content item";

            AddScalarFilterFields<StringGraphType>("alias", "the alias of the content item");
        }
    }
}
