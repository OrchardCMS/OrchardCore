using System;
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
                var name = FirstCharacterToLower(propertyInfo.Name);

                if (resolveFieldContext.HasPopulatedArgument(name))
                {
                    propertyInfo.SetValue(
                        value,
                        resolveFieldContext.GetArgument(propertyInfo.PropertyType, name));
                }
            }

            return value;
        }

        private static string FirstCharacterToLower(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
                return str;

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
