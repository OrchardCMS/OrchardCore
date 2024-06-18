using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public interface IIndexAliasProvider
    {
        ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync();
    }
}
