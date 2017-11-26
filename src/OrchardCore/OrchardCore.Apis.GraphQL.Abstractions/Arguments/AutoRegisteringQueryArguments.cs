using System.Linq;
using System.Reflection;
using GraphQL;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Arguments
{
    public class AutoRegisteringQueryArguments<TSourceType> : QueryArguments
    {
        public AutoRegisteringQueryArguments(
            string[] requiredProperties = null,
            string[] propertiesToExclude = null)
        {
            if (requiredProperties == null)
            {
                requiredProperties = new string[0];
            }
            if (propertiesToExclude == null)
            {
                propertiesToExclude = new string[0];
            }

            var properties = typeof(TSourceType).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.PropertyType.GetTypeInfo().IsValueType || p.PropertyType == typeof(string));

            foreach (var propertyInfo in properties)
            {
                if (propertiesToExclude.Contains(propertyInfo.Name))
                {
                    continue;
                }

                Add(new QueryArgument(propertyInfo.PropertyType.GetGraphTypeFromType(!requiredProperties.Contains(propertyInfo.Name))) { Name = propertyInfo.Name });
            }
        }
    }
}
