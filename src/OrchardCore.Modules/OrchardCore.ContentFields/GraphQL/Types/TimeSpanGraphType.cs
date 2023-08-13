using System;
using System.Globalization;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace OrchardCore.ContentFields.GraphQL.Types
{
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
            return String.IsNullOrWhiteSpace(timespan)
                ? null
                : (TimeSpan?)TimeSpan.Parse(timespan, CultureInfo.CurrentCulture);
        }

        public override object ParseLiteral(IValue value)
        {
            var str = value as StringValue;
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
            if (!String.IsNullOrEmpty(value) && value.Length > 2 && value.StartsWith('\"') && value.EndsWith('\"'))
            {
                return value[1..^1];
            }

            return value;
        }
    }
}
