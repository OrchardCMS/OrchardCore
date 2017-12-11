using System.Linq;
using System.Reflection;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Arguments
{
    public static class ResolveFieldContextExtensions
    {
        public static TSourceType MapArgumentsTo<TSourceType>(this ResolveFieldContext<object> resolveFieldContext)
            where TSourceType : new()
        {
            var value = new TSourceType();

            var properties = typeof(TSourceType).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.GetTypeInfo().IsValueType || p.PropertyType == typeof(string));

            foreach (var propertyInfo in properties)
            {
                if (resolveFieldContext.HasPopulatedArgument(propertyInfo.Name))
                {
                    propertyInfo.SetValue(
                        value,
                        resolveFieldContext.GetArgument(propertyInfo.PropertyType, propertyInfo.Name));
                }
            }

            return value;
        }
    }
}
