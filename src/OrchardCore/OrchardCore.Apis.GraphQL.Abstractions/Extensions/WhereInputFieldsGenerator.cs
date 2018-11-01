using System;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL
{
    public static class WhereInputFieldsGenerator
    {
        // Applies to all types
        public static string[] EqualityOperators = { "", "_not" };

        public static string[] NonStringValueComparisonOperators = { "_gt", "_gte", "_lt", "_lte" };

        // Applies to strings
        public static string[] StringComparisonOperators = { "_contains", "_not_contains", "_starts_with", "_not_starts_with", "_ends_with", "_not_ends_with" };

        // Applies to all types
        public static string[] MultiValueComparisonOperators = { "_in", "_not_in" };

        public static void AddScalarFilterFields(this IComplexGraphType input, Type graphType, string fieldName, string description)
        {
            BuildEqualityFilters(input, graphType, fieldName, description);
            BuildMultiValueFilters(input, graphType, fieldName, description);

            if (graphType == typeof(StringGraphType))
            {
                BuildStringFilters(input, graphType, fieldName, description);
            }
            else if (graphType == typeof(DateTimeGraphType))
            {
                BuildNonStringFilters(input, graphType, fieldName, description);
            }
        }

        private static void BuildEqualityFilters(IComplexGraphType input, Type graphType, string fieldName, string description)
        {
            AddFilterFields(input, graphType, EqualityOperators, fieldName, description);
        }

        private static void BuildStringFilters(IComplexGraphType input, Type graphType, string fieldName, string description)
        {
            AddFilterFields(input, graphType, StringComparisonOperators, fieldName, description);
        }

        private static void BuildNonStringFilters(IComplexGraphType input, Type graphType, string fieldName, string description)
        {
            AddFilterFields(input, graphType, NonStringValueComparisonOperators, fieldName, description);
        }

        private static void BuildMultiValueFilters(IComplexGraphType input, Type graphType, string fieldName, string description)
        {
            var wrappedType = typeof(ListGraphType<>).MakeGenericType(graphType);
            AddFilterFields(input, wrappedType, MultiValueComparisonOperators, fieldName, description);
        }

        private static void AddFilterFields(IComplexGraphType input, Type graphType, string[] filters, string fieldName, string description)
        {
            foreach (var comparison in filters)
            {
                input.AddField(new FieldType 
                { 
                    Type = graphType, 
                    Name = fieldName + comparison, 
                    Description = description
                });
            }
        }
    }
}