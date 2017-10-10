using System;
using System.Linq;
using System.Reflection;
using GraphQL;
using GraphQL.Types;

namespace OrchardCore.RestApis.Queries.Types
{
    public class AutoRegisteringInputObjectGraphType : InputObjectGraphType
    {
        public AutoRegisteringInputObjectGraphType(string name, Type type)
        {
            Name = name;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.PropertyType.GetTypeInfo().IsValueType || p.PropertyType == typeof(string));

            foreach (var propertyInfo in properties)
            {
                Field(propertyInfo.PropertyType.GetGraphTypeFromType(propertyInfo.PropertyType.IsNullable()), propertyInfo.Name);
            }
        }
    }
}
