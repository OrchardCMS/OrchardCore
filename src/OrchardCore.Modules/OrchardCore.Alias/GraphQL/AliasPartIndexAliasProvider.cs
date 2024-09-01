using OrchardCore.Alias.Indexes;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace OrchardCore.Alias.GraphQL;

public class AliasPartIndexAliasProvider : IIndexAliasProvider
{
    private static readonly IndexAlias[] _aliases =
    [
        new IndexAlias
        {
            Alias = "aliasPart",
            Index = "AliasPartIndex",
            IndexType = typeof(AliasPartIndex)
        }
    ];

    public ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
    {
        return ValueTask.FromResult<IEnumerable<IndexAlias>>(_aliases);
    }
}
