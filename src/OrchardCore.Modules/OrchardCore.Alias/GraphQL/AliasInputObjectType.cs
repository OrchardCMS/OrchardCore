using GraphQL.Types;
using OrchardCore.Alias.Models;
using OrchardCore.Apis.GraphQL;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasInputObjectType : InputObjectGraphType<AliasPart>
    {
        public AliasInputObjectType()
        {
            Name = "AliasPartInput";
            Description = "the alias part of the content item";

            this.AddScalarFilterFields(typeof(StringGraphType), "alias", "the alias of the content item to filter");
        }
    }
}
