using System.Collections.Generic;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.Records;

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
                With = q => q.With<AutoroutePartIndex>()
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }
    }
}
