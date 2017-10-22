using System.Collections.Generic;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public class QueriesSchema : ObjectGraphType
    {
        public QueriesSchema(
            IEnumerable<QueryFieldType> queryFields,
            IDynamicQueryFieldTypeProvider provider)
        {
            Name = "Query";

            foreach (var queryField in queryFields)
            {
                AddField(queryField);
            }

            foreach (var queryField in provider.GetFields())
            {
                AddField(queryField);
            }
        }
    }
}
