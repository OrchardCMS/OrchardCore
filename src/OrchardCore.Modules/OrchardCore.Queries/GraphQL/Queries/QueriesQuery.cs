using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Queries.GraphQL.Queries
{
    public class QueriesQuery<T> : QueryFieldType
    {
        public QueriesQuery()
        {
            Name = typeof(T).Name;
            Type = typeof(T);
        }
    }
}
