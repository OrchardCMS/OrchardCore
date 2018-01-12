using OrchardCore.Alias.Models;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasInputObjectType : QueryArgumentObjectGraphType<AliasPart>
    {
        public AliasInputObjectType()
        {
            Name = "AliasPartInput";

            AddInputField("alias", x => x.Alias, true);
        }
    }
}
