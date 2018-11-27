using GraphQL.Types;
using OrchardCore.Alias.Models;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasQueryObjectType : ObjectGraphType<AliasPart>
    {
        public AliasQueryObjectType()
        {
            Name = "AliasPart";


            Field("alias", x => x.Alias, true);
        }
    }
}
