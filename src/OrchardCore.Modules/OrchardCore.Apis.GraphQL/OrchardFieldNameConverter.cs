using System;
using GraphQL;
using GraphQL.Conversion;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL
{
    public class OrchardFieldNameConverter : INameConverter
    {
        private readonly INameConverter _defaultConverter = new CamelCaseNameConverter();

        public string NameForArgument(string fieldName, IComplexGraphType parentGraphType, FieldType field)
        {
            return _defaultConverter.NameForArgument(fieldName, parentGraphType, field);
        }

        public string NameForField(string fieldName, IComplexGraphType parentGraphType)
        {
            var attributes = parentGraphType?.GetType().GetCustomAttributes(typeof(GraphQLFieldNameAttribute), true);

            if (attributes != null)
            {
                foreach (GraphQLFieldNameAttribute attribute in attributes)
                {

                    if (attribute.Field == fieldName)
                    {
                        return attribute.Mapped;
                    }
                }
            }

            return _defaultConverter.NameForField(fieldName, parentGraphType);
        }
    }
}
