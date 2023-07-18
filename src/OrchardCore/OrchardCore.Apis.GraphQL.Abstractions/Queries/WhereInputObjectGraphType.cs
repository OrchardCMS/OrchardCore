using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public class WhereInputObjectGraphType : WhereInputObjectGraphType<object>
    {
    }

    public class WhereInputObjectGraphType<TSourceType> : InputObjectGraphType<TSourceType>
    {
        // arguments of typed input graph types return typed object, without additional input fields (_in, _contains,..)
        // so we return dictionary as it was before.
        public override object ParseDictionary(IDictionary<string, object> value)
        {
            return value;
        }

        // Applies to all types.
        public static readonly Dictionary<string, string> EqualityOperators = new()
        {
            { "", "is equal to" },
            { "_not", "is not equal to" },
        };

        // Applies to all types.
        public static readonly Dictionary<string, string> MultiValueComparisonOperators = new()
        {
            { "_in", "is in collection" },
            { "_not_in", "is not in collection" },
        };

        // Applies to non strings.
        public static readonly Dictionary<string, string> NonStringValueComparisonOperators = new()
        {
            { "_gt", "is greater than" },
            { "_gte", "is greater than or equal" },
            { "_lt", "is less than" },
            { "_lte", "is less than or equal" },
        };

        // Applies to strings.
        public static readonly Dictionary<string, string> StringComparisonOperators = new()
        {
            {"_contains", "contains the string"},
            {"_not_contains", "does not contain the string"},
            {"_starts_with", "starts with the string"},
            {"_not_starts_with", "does not start with the string"},
            {"_ends_with", "ends with the string"},
            {"_not_ends_with", "does not end with the string"},
        };

        public void AddScalarFilterFields<TGraphType>(string fieldName, string description)
        {
            AddScalarFilterFields(typeof(TGraphType), fieldName, description);
        }

        public void AddScalarFilterFields(Type graphType, string fieldName, string description)
        {
            AddEqualityFilters(graphType, fieldName, description);
            AddMultiValueFilters(graphType, fieldName, description);

            if (graphType == typeof(StringGraphType))
            {
                AddStringFilters(graphType, fieldName, description);
            }
            else if (graphType == typeof(DateTimeGraphType))
            {
                AddNonStringFilters(graphType, fieldName, description);
            }
        }

        private void AddEqualityFilters(Type graphType, string fieldName, string description)
        {
            AddFilterFields(graphType, EqualityOperators, fieldName, description);
        }

        private void AddStringFilters(Type graphType, string fieldName, string description)
        {
            AddFilterFields(graphType, StringComparisonOperators, fieldName, description);
        }

        private void AddNonStringFilters(Type graphType, string fieldName, string description)
        {
            AddFilterFields(graphType, NonStringValueComparisonOperators, fieldName, description);
        }

        private void AddMultiValueFilters(Type graphType, string fieldName, string description)
        {
            var wrappedType = typeof(ListGraphType<>).MakeGenericType(graphType);
            AddFilterFields(wrappedType, MultiValueComparisonOperators, fieldName, description);
        }

        private void AddFilterFields(Type graphType, IDictionary<string, string> filters, string fieldName, string description)
        {
            foreach (var filter in filters)
            {
                Field(graphType, fieldName + filter.Key, $"{description} {filter.Value}");
            }
        }
    }
}
