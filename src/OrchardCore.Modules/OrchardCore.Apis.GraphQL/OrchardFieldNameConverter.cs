using System.Linq;
using GraphQL.Conversion;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL
{
    public class OrchardFieldNameConverter : INameConverter
    {
        private readonly INameConverter _defaultConverter = new CamelCaseNameConverter();

        // todo: custom argument name?
        public string NameForArgument(string argumentName, IComplexGraphType parentGraphType, FieldType field)
        {
            return _defaultConverter.NameForArgument(argumentName, parentGraphType, field);
        }

        // TODO: check functionality.
        public string NameForField(string fieldName, IComplexGraphType parentGraphType)
        {
            var attributes = parentGraphType?.GetType().GetCustomAttributes(typeof(GraphQLFieldNameAttribute), true);

            if (attributes != null)
            {
                foreach (var attribute in attributes.Cast<GraphQLFieldNameAttribute>())
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
