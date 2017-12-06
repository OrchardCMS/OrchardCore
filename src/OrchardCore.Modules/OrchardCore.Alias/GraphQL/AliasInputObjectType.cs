using GraphQL.Types;
using OrchardCore.Alias.Models;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasInputObjectType : InputObjectGraphType<AliasPart>
    {
        public AliasInputObjectType()
        {
            Name = "AliasPartInput";

            Field(x => x.Alias, false);
        }
    }
}
