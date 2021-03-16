using System;
using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Options
{
    public class GraphQLField<IGraphType> : GraphQLField where IGraphType : IObjectGraphType
    {
        public GraphQLField(string fieldName) : base(typeof(IGraphType), fieldName)
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
