using System;
using GraphQL.Conversion;

namespace OrchardCore.Apis.GraphQL
{
    public class OrchardFieldNameConverter : IFieldNameConverter
    {
        private IFieldNameConverter defaultConverter = new CamelCaseFieldNameConverter();

        public string NameFor(string field, Type parentType)
        {
            var attributes = parentType?.GetCustomAttributes(typeof(GraphQLFieldNameAttribute), true);

            if (attributes != null)
            {
                foreach(GraphQLFieldNameAttribute attribute in attributes)
                {
                    if (attribute.Field == field)
                    {
                        return attribute.Mapped;
                    }
                }
            }

            return defaultConverter.NameFor(field, parentType);
        }
    }
}
