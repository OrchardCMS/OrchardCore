using System.Collections.Generic;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace OrchardCore.ContentLocalization.GraphQL
{
    public class LocalizationPartIndexAliasProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
        {
            new IndexAlias
            {
                Alias = "localizationPart",
                Index = nameof(LocalizedContentItemIndex),
                IndexType = typeof(LocalizedContentItemIndex)
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }
    }
}
