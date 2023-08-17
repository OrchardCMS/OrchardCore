using System;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class StringEqualsOperatorComparer : OperatorComparer<StringOperator, string>
    {
        public override bool Compare(StringOperator conditionOperator, string a, string b)
            => conditionOperator.CaseSensitive ?
                String.Equals(a, b) :
                String.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public class StringNotEqualsOperatorComparer : OperatorComparer<StringOperator, string>
    {
        public override bool Compare(StringOperator conditionOperator, string a, string b)
            => conditionOperator.CaseSensitive ?
                !String.Equals(a, b) :
                !String.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public class StringStartsWithOperatorComparer : OperatorComparer<StringOperator, string>
    {
        public override bool Compare(StringOperator conditionOperator, string a, string b)
            => conditionOperator.CaseSensitive ?
                a.StartsWith(b) :
                a.StartsWith(b, StringComparison.OrdinalIgnoreCase);
    }

    public class StringNotStartsWithOperatorComparer : OperatorComparer<StringOperator, string>
    {
        public override bool Compare(StringOperator conditionOperator, string a, string b)
            => conditionOperator.CaseSensitive ?
                !a.StartsWith(b) :
                !a.StartsWith(b, StringComparison.OrdinalIgnoreCase);
    }

    public class StringEndsWithOperatorComparer : OperatorComparer<StringOperator, string>
    {
        public override bool Compare(StringOperator conditionOperator, string a, string b)
            => conditionOperator.CaseSensitive ?
                a.EndsWith(b) :
                a.EndsWith(b, StringComparison.OrdinalIgnoreCase);
    }

    public class StringNotEndsWithOperatorComparer : OperatorComparer<StringOperator, string>
    {
        public override bool Compare(StringOperator conditionOperator, string a, string b)
            => conditionOperator.CaseSensitive ?
                !a.EndsWith(b) :
                !a.EndsWith(b, StringComparison.OrdinalIgnoreCase);
    }

    public class StringContainsOperatorComparer : OperatorComparer<StringOperator, string>
    {
        public override bool Compare(StringOperator conditionOperator, string a, string b)
            => conditionOperator.CaseSensitive ?
                a.Contains(b) :
                a.Contains(b, StringComparison.OrdinalIgnoreCase);
    }

    public class StringNotContainsOperatorComparer : OperatorComparer<StringOperator, string>
    {
        public override bool Compare(StringOperator conditionOperator, string a, string b)
            => conditionOperator.CaseSensitive ?
                !a.Contains(b) :
                !a.Contains(b, StringComparison.OrdinalIgnoreCase);
    }
}
