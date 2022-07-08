using System.Collections.Generic;
using OrchardCore.Alias.Indexes;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasPartIndexAliasProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
        {
            new IndexAlias
            {
                Alias = "aliasPart",
                Index = "AliasPartIndex",
                IndexType = typeof(AliasPartIndex)
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }
    }
}
