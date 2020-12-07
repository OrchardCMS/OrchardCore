using System;
using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Options
{
    public class GraphQLField<TGraphType> : GraphQLField where TGraphType : IObjectGraphType
    {
        public GraphQLField(string fieldName) : base(typeof(TGraphType), fieldName)
        {
        }
    }

    public class GraphQLField
    {
        public GraphQLField(Type fieldType, string fieldName)
        {
            if (fieldType == null)
            {
                throw new ArgumentNullException(nameof(fieldType));
            }

            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentNullException(nameof(fieldName));
            }

            FieldName = fieldName;
            FieldType = fieldType;
        }

        public string FieldName { get; }

        public Type FieldType { get; }
    }
}
