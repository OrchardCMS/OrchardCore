using System.Linq;
using System.Reflection;
using GraphQL;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Types
{
    public class AutoRegisteringInputObjectGraphType<TSourceType> : InputObjectGraphType<TSourceType>
    {
        public AutoRegisteringInputObjectGraphType()
        {
            var properties = typeof(TSourceType).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.PropertyType.GetTypeInfo().IsValueType || p.PropertyType == typeof(string));

            foreach (var propertyInfo in properties)
            {
                Field(propertyInfo.PropertyType.GetGraphTypeFromType(propertyInfo.PropertyType.IsNullable()), propertyInfo.Name);
            }
        }
    }
}
