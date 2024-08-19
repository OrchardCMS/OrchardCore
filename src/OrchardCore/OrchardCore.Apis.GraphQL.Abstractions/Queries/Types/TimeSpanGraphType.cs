using System.Globalization;
using GraphQL.Types;
using GraphQLParser.AST;

namespace OrchardCore.Apis.GraphQL.Queries.Types;

public class TimeSpanGraphType : ScalarGraphType
{
    public TimeSpanGraphType()
    {
        Name = "TimeSpan";
        Description = "Represents a time interval.";
    }

    public override object Serialize(object value)
    {
        return value?.ToString();
    }

    public override object ParseValue(object value)
    {
        var timespan = value?.ToString().StripQuotes();
        return string.IsNullOrWhiteSpace(timespan)
            ? null
            : (TimeSpan?)TimeSpan.Parse(timespan, CultureInfo.CurrentCulture);
    }

    public override object ParseLiteral(GraphQLValue value)
    {
        var str = value as GraphQLStringValue;
        if (str != null)
        {
            return ParseValue(str.Value);
        }

        return null;
    }
}

public static class ScalarGraphTypeExtensions
{
    public static string StripQuotes(this string value)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > 2 && value.StartsWith('\"') && value.EndsWith('\"'))
        {
            return value[1..^1];
        }

        return value;
    }
}
