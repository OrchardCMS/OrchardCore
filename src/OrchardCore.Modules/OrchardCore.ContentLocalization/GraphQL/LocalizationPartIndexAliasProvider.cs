using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace OrchardCore.ContentLocalization.GraphQL;

public class LocalizationPartIndexAliasProvider : IIndexAliasProvider
{
    private static readonly IndexAlias[] s_aliases =
    [
        new IndexAlias
        {
            Alias = "localizationPart",
            Index = nameof(LocalizedContentItemIndex),
            IndexType = typeof(LocalizedContentItemIndex),
        }
    ];

    public ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
    {
        return ValueTask.FromResult<IEnumerable<IndexAlias>>(s_aliases);
    }
}
