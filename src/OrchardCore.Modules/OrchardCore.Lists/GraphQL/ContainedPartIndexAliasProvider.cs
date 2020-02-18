using System.Collections.Generic;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Lists.Indexes;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedPartIndexAliasProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
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
            return _aliases;
        }
    }
}
