using System.Linq;
using System.Reflection;
using GraphQL;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Arguments
{
    public class AutoRegisteringQueryArguments<TSourceType> : QueryArguments
    {
        public AutoRegisteringQueryArguments()
        {
            var properties = typeof(TSourceType).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.PropertyType.GetTypeInfo().IsValueType || p.PropertyType == typeof(string));

            foreach (var propertyInfo in properties)
            {
                Add(new QueryArgument(propertyInfo.PropertyType.GetGraphTypeFromType(true)) { Name = propertyInfo.Name });
            }
        }
    }
}
