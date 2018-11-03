using System.Collections.Generic;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.Records;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutoroutePartIndexAliasProvider : IIndexAliasProvider
    {
        public IEnumerable<IndexAlias> GetAliases()
        {
            return new[]
            {
                new IndexAlias
                {
                    Alias = "autoroutePart",
                    Index = nameof(AutoroutePartIndex),
                    With = q => q.With<AutoroutePartIndex>()
                }
            };
        }
    }
}
