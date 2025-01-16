using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries.Types;

namespace OrchardCore.Apis.GraphQL.Queries;

public abstract class WhereInputObjectGraphType : WhereInputObjectGraphType<object>
{
    protected WhereInputObjectGraphType(IStringLocalizer stringLocalizer)
        : base(stringLocalizer)
    {
    }
}

public abstract class WhereInputObjectGraphType<TSourceType> : InputObjectGraphType<TSourceType>, IFilterInputObjectGraphType
{
    protected readonly IStringLocalizer S;

    protected WhereInputObjectGraphType(IStringLocalizer stringLocalizer)
    {
        S = stringLocalizer;
    }

    // arguments of typed input graph types return typed object, without additional input fields (_in, _contains,..)
    // so we return dictionary as it was before.
    public override object ParseDictionary(IDictionary<string, object> value)
    {
        return value;
    }

    // Applies to all types.
    public static readonly Dictionary<string, Func<IStringLocalizer, string, string>> EqualityOperators = new()
    {
        { "",  (S, description) => S["{0} is equal to", description] },
        { "_not",  (S, description) => S["{0} is not equal to", description] },
    };

    // Applies to all types.
    public static readonly Dictionary<string, Func<IStringLocalizer, string, string>> MultiValueComparisonOperators = new()
    {
        { "_in", (S, description) => S["{0} is in collection", description] },
        { "_not_in", (S, description) => S["{0} is not in collection", description] },
    };

    // Applies to non strings.
    public static readonly Dictionary<string, Func<IStringLocalizer, string, string>> NonStringValueComparisonOperators = new()
    {
        { "_gt", (S, description) => S["{0} is greater than", description] },
        { "_gte", (S, description) => S["{0} is greater than or equal", description] },
        { "_lt", (S, description) => S["{0} is less than", description] },
        { "_lte", (S, description) => S["{0} is less than or equal", description] },
    };

    // Applies to strings.
    public static readonly Dictionary<string, Func<IStringLocalizer, string, string>> StringComparisonOperators = new()
    {
        { "_contains", (S, description) => S["{0} contains the string", description] },
        { "_not_contains", (S, description) => S["{0} does not contain the string", description] },
        { "_starts_with", (S, description) => S["{0} starts with the string", description] },
        { "_not_starts_with", (S, description) => S["{0} does not start with the string", description] },
        { "_ends_with", (S, description) => S["{0} ends with the string", description] },
        { "_not_ends_with", (S, description) => S["{0} does not end with the string", description] },
    };

    public void AddScalarFilterFields<TGraphType>(string fieldName, string description)
        => AddScalarFilterFields<TGraphType>(fieldName, description, null);

    public virtual void AddScalarFilterFields<TGraphType>(string fieldName, string description, string aliasName)
    {
        AddScalarFilterFields(typeof(TGraphType), fieldName, description, aliasName);
    }

    public void AddScalarFilterFields(Type graphType, string fieldName, string description)
        => AddScalarFilterFields(graphType, fieldName, description, null);

    public virtual void AddScalarFilterFields(Type graphType, string fieldName, string description, string aliasName)
    {
        if (!typeof(ScalarGraphType).IsAssignableFrom(graphType) &&
            !typeof(IInputObjectGraphType).IsAssignableFrom(graphType))
        {
            return;
        }

        AddEqualityFilters(graphType, fieldName, description, aliasName);

        if (graphType == typeof(StringGraphType))
        {
            AddMultiValueFilters(graphType, fieldName, description, aliasName);
            AddStringFilters(graphType, fieldName, description, aliasName);
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
            AddMultiValueFilters(graphType, fieldName, description, aliasName);
            AddNonStringFilters(graphType, fieldName, description, aliasName);
        }
    }

    private void AddEqualityFilters(Type graphType, string fieldName, string description, string aliasName)
    {
        AddFilterFields(graphType, EqualityOperators, fieldName, description, aliasName);
    }

    private void AddStringFilters(Type graphType, string fieldName, string description, string aliasName)
    {
        AddFilterFields(graphType, StringComparisonOperators, fieldName, description, aliasName);
    }

    private void AddNonStringFilters(Type graphType, string fieldName, string description, string aliasName)
    {
        AddFilterFields(graphType, NonStringValueComparisonOperators, fieldName, description, aliasName);
    }

    private void AddMultiValueFilters(Type graphType, string fieldName, string description, string aliasName)
    {
        AddFilterFields(graphType, MultiValueComparisonOperators, fieldName, description, aliasName);
    }

    private void AddFilterFields(
        Type graphType,
        IDictionary<string, Func<IStringLocalizer, string, string>> filters,
        string fieldName,
        string description,
        string aliasName)
    {
        foreach (var filter in filters)
        {
            AddField(new FieldType
            {
                Name = fieldName + filter.Key,
                Description = filter.Value(S, description),
                Type = graphType,
            }.WithAliasNameMetaData(aliasName));
        }
    }
}
