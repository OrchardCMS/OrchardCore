using System.Collections.Generic;
using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutoroutePartIndexAliasProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
        {
            new IndexAlias
            {
                Alias = "autoroutePart",
                Index = nameof(AutoroutePartIndex),
                IndexType = typeof(AutoroutePartIndex)
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }
    }
}
