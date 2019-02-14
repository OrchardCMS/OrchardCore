using System;

namespace OrchardCore.ContentManagement.GraphQL.Options
{
    public class GraphQLField<IObjectGraphType> : GraphQLField where IObjectGraphType : new()
    {
        public GraphQLField(string fieldName) : base (typeof(IObjectGraphType), fieldName)
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