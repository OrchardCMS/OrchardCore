using System.Collections.Generic;
using System.Linq.Expressions;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using YesSql;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public interface IWhereFilter<TSourceType> where TSourceType : class
    {
        void OnBeforeQuery(IQuery<TSourceType> query);

        bool TryGetPropertyComparison(JProperty property, out Expression left, out Expression right);
    }

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
