using System.Collections.Generic;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Lists.Indexes;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedPartIndexAliasProvider : IIndexAliasProvider
    {
        private static IndexAlias[] Aliases = new []
        {
            new IndexAlias
            {
                Alias = "containedPart",
                Index = nameof(ContainedPartIndex),
                With = q => q.With<ContainedPartIndex>()
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return Aliases;
        }
    }
}
