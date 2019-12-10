using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Types;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public abstract class GraphQLFilter<TSourceType> : IGraphQLFilter<TSourceType> where TSourceType : class
    {
        public virtual Task<IQuery<TSourceType>> PreQueryAsync(IQuery<TSourceType> query, ResolveFieldContext context)
        {
            return Task.FromResult(query);
        }

        public virtual Task<IEnumerable<TSourceType>> PostQueryAsync(IEnumerable<TSourceType> contentItems, ResolveFieldContext context)
        {
            return Task.FromResult(contentItems);
        }
    }
}
