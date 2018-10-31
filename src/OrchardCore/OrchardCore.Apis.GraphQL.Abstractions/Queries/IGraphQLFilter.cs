using System.Collections.Generic;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using YesSql;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public interface IGraphQLFilter<TSourceType> where TSourceType : class
    {
        IQuery<TSourceType> PreQuery(IQuery<TSourceType> query, ResolveFieldContext context, JObject whereArguments);

        IEnumerable<TSourceType> PostQuery(IEnumerable<TSourceType> contentItems, ResolveFieldContext context);
    }
}
