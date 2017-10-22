using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Types;

namespace OrchardCore.RestApis.GraphQL.Queries
{
    public class QueriesSchema : ObjectGraphType
    {
        public QueriesSchema(IEnumerable<QueryFieldType> queryFields)
        {
            Name = "Query";

            foreach (var queryField in queryFields)
            {
                AddField(queryField);
            }
        }
    }
}
