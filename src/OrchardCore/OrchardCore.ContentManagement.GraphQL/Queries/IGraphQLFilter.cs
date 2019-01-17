using System.Collections.Generic;
using GraphQL.Types;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public interface IGraphQLFilter<TSourceType> where TSourceType : class
    {
        IQuery<TSourceType> PreQuery(IQuery<TSourceType> query, ResolveFieldContext context);

        IEnumerable<TSourceType> PostQuery(IEnumerable<TSourceType> contentItems, ResolveFieldContext context);
    }
}
