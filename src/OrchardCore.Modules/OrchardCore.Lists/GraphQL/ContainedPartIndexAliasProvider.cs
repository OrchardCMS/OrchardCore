using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Lists.Indexes;

namespace OrchardCore.Lists.GraphQL;

public class ContainedPartIndexAliasProvider : IIndexAliasProvider
{
    private static readonly IndexAlias[] _aliases =
    [
        new IndexAlias
        {
            Alias = "containedPart",
            Index = nameof(ContainedPartIndex),
            IndexType = typeof(ContainedPartIndex)
        }
    ];

    public ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
    {
        return ValueTask.FromResult<IEnumerable<IndexAlias>>(_aliases);
    }
}
