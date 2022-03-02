using System.Collections.Generic;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Seo.Indexes;

namespace OrchardCore.Seo.GraphQL;

public class SeoMetaPartIndexAliasProvider : IIndexAliasProvider
{
    private static readonly IndexAlias[] _aliases = new[]
    {
        new IndexAlias
        {
            Alias = "seoMetaPart",
            Index = nameof(SeoMetaPartIndex),
            IndexType = typeof(SeoMetaPartIndex)
        }
    };

    public IEnumerable<IndexAlias> GetAliases()
    {
        return _aliases;
    }
}
