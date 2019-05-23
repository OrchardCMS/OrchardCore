using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Types;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public interface IGraphQLFilter<TSourceType> where TSourceType : class
    {
        Task<IQuery<TSourceType>> PreQueryAsync(IQuery<TSourceType> query, ResolveFieldContext context);

        Task<IEnumerable<TSourceType>> PostQueryAsync(IEnumerable<TSourceType> contentItems, ResolveFieldContext context);
    }
}
