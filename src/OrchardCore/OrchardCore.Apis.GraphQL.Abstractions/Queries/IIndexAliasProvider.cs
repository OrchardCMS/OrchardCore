using System.Collections.Generic;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public interface IIndexAliasProvider
    {
        IEnumerable<IndexAlias> GetAliases();
    }
}