using GraphQL.Types;
using OrchardCore.Alias.Models;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasInputObjectType : InputObjectGraphType<AliasPart>
    {
        public AliasInputObjectType()
        {
            Name = "AliasPartInput";

            Field(x => x.Alias, nullable: true)
                .Name("alias")
                .Type(new StringGraphType())
                .Description("the alias of the content item to filter")
                ;
        }
    }
}
