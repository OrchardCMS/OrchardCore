using System.Collections.Generic;
using GraphQL.Types;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public abstract class GraphQLFilter<TSourceType> : IGraphQLFilter<TSourceType> where TSourceType : class
    {
        public virtual IQuery<TSourceType> PreQuery(IQuery<TSourceType> query, ResolveFieldContext context)
        {
            return query;
        }

        public virtual IEnumerable<TSourceType> PostQuery(IEnumerable<TSourceType> contentItems, ResolveFieldContext context)
        {
            return contentItems;
        }
    }
}
