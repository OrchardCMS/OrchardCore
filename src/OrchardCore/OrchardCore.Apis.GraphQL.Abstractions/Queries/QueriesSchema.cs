using System.Collections.Generic;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public class QueriesSchema : ObjectGraphType
    {
        public QueriesSchema(
            IEnumerable<QueryFieldType> queryFields,
            IEnumerable<IQueryFieldTypeProvider> queryFieldTypeProviders)
        {
            Name = "Query";

            foreach (var queryField in queryFields)
            {
                AddField(queryField);
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
