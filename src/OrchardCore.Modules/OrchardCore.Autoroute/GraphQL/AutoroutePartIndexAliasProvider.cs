using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace OrchardCore.Autoroute.GraphQL;

public class AutoroutePartIndexAliasProvider : IIndexAliasProvider
{
    private static readonly IndexAlias[] _aliases =
    [
        new IndexAlias
        {
            Alias = "autoroutePart",
            Index = nameof(AutoroutePartIndex),
            IndexType = typeof(AutoroutePartIndex)
        }
    ];

    public ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
    {
        return ValueTask.FromResult<IEnumerable<IndexAlias>>(_aliases);
    }
}
