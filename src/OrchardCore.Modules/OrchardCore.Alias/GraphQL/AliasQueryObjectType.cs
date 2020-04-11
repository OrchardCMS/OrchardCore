using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Models;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasQueryObjectType : ObjectGraphType<AliasPart>
    {
        public AliasQueryObjectType(IStringLocalizer<AliasQueryObjectType> S)
        {
            Name = "AliasPart";
            Description = S["Alternative path for the content item"];

            Field("alias", x => x.Alias, true);
        }
    }
}
