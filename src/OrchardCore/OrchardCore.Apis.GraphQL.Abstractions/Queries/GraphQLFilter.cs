using System.Collections.Generic;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using YesSql;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public abstract class GraphQLFilter<TSourceType> : IGraphQLFilter<TSourceType> where TSourceType : class
    {
        public virtual IQuery<TSourceType> PreQuery(IQuery<TSourceType> query, ResolveFieldContext context,
            JObject whereArguments)
        {
            return query;
        }

        public virtual IEnumerable<TSourceType> PostQuery(IEnumerable<TSourceType> contentItems, ResolveFieldContext context)
        {
            return contentItems;
        }
    }
}
