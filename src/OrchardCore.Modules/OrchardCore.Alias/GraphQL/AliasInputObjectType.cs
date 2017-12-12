using GraphQL.Types;
using OrchardCore.Alias.Models;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasInputObjectType : InputObjectGraphType<AliasPart>, IQueryArgumentObjectGraphType
    {
        public AliasInputObjectType()
        {
            Name = "AliasPartInput";

            this.AddInputField("alias", x => x.Alias, true);
        }
    }
}
