using System.Collections.Generic;
using OrchardCore.Alias.Indexes;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasPartIndexAliasProvider : IIndexAliasProvider
    {
        public IEnumerable<IndexAlias> GetAliases()
        {
            return new[]
            {
                new IndexAlias
                {
                    Alias = "aliasPart",
                    Index = "AliasPartIndex",
                    With = q => q.With<AliasPartIndex>()
                }
            };
        }
    }
}
