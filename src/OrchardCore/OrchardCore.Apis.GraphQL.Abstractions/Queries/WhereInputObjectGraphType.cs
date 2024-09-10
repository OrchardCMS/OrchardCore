using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries.Types;

namespace OrchardCore.Apis.GraphQL.Queries;

public interface IFilterInputObjectGraphType : IInputObjectGraphType
{
    void AddScalarFilterFields<TGraphType>(string fieldName, string description);

    void AddScalarFilterFields(Type graphType, string fieldName, string description);
}

public class WhereInputObjectGraphType : WhereInputObjectGraphType<object>, IFilterInputObjectGraphType
{
}

public class WhereInputObjectGraphType<TSourceType> : InputObjectGraphType<TSourceType>, IFilterInputObjectGraphType
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

    public virtual void AddScalarFilterFields<TGraphType>(string fieldName, string description)
    {
        AddScalarFilterFields(typeof(TGraphType), fieldName, description);
    }

    public virtual void AddScalarFilterFields(Type graphType, string fieldName, string description)
    {
        if (!typeof(ScalarGraphType).IsAssignableFrom(graphType) &&
            !typeof(IInputObjectGraphType).IsAssignableFrom(graphType))
        {
            return;
        }

        AddEqualityFilters(graphType, fieldName, description);

        if (graphType == typeof(StringGraphType))
        {
            AddMultiValueFilters(graphType, fieldName, description);
            AddStringFilters(graphType, fieldName, description);
        }
        else if (graphType == typeof(DateTimeGraphType) ||
            graphType == typeof(DateGraphType) ||
            graphType == typeof(DateOnlyGraphType) ||
            graphType == typeof(TimeSpanGraphType) ||
            graphType == typeof(DecimalGraphType) ||
            graphType == typeof(IntGraphType) ||
            graphType == typeof(LongGraphType) ||
            graphType == typeof(FloatGraphType) ||
            graphType == typeof(BigIntGraphType))
        {
            AddMultiValueFilters(graphType, fieldName, description);
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
            AddField(new FieldType
            {
                Name = fieldName + filter.Key,
                Description = $"{description} {filter.Value}",
                Type = graphType
            });
        }
    }
}
