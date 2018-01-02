using System.Collections.Generic;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public class QueriesSchema : ObjectGraphType
    {
        public QueriesSchema(
            IEnumerable<QueryFieldType> fields,
            IEnumerable<IQueryFieldTypeProvider> queryFieldTypeProviders)
        {
            Name = "Query";

            foreach (var field in fields)
            {
                AddField(field);
            }

            foreach (var provider in queryFieldTypeProviders)
            {
                foreach (var queryField in provider.GetFields(this).GetAwaiter().GetResult())
                {
                    AddField(queryField);
                }
            }
        }
    }
}
