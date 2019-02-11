using System;

namespace OrchardCore.ContentManagement.GraphQL.Options
{
    public class GraphQLField
    {
        public string FieldName { get; set; }

        public Type FieldType { get; set; }
    }
}