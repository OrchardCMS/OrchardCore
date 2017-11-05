using System.Collections.Generic;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public class QueriesSchema : ObjectGraphType
    {
        public QueriesSchema(
            IEnumerable<QueryFieldType> queryFields,
            IEnumerable<IDynamicQueryFieldTypeProvider> dynamicQueryFieldTypeProviders)
        {
            Name = "Query";

            foreach (var queryField in queryFields)
            {
                AddField(queryField);
            }

            foreach (var dynamicQueryFieldTypeProvider in dynamicQueryFieldTypeProviders)
            {
                foreach (var queryField in dynamicQueryFieldTypeProvider.GetFields())
                {
                    AddField(queryField);
                }
            }
        }
    }
}
