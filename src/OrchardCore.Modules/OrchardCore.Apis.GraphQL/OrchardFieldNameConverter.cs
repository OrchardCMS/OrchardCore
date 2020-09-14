using System;
using GraphQL.Conversion;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL
{
    public class OrchardFieldNameConverter : INameConverter
    {
        private readonly INameConverter _defaultConverter = new CamelCaseNameConverter();

        public string NameForField(string fieldName, IComplexGraphType parentGraphType)
        {
            //var attributes = parentGraphType?.GetCustomAttributes(typeof(GraphQLFieldNameAttribute), true);

            //if (attributes != null)
            //{
            //    foreach (GraphQLFieldNameAttribute attribute in attributes)
            //    {
            //        if (attribute.Field == fieldName)
            //        {
            //            return attribute.Mapped;
            //        }
            //    }
            //}

            return _defaultConverter.NameForField(fieldName, parentGraphType);
        }

        public string NameForArgument(string argumentName, IComplexGraphType parentGraphType, FieldType field)
        {
            throw new NotImplementedException();
        }

    }
}
