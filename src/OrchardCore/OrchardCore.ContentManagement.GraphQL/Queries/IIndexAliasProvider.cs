using System.Collections.Generic;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public interface IIndexAliasProvider
    {
        IEnumerable<IndexAlias> GetAliases();
    }
}
