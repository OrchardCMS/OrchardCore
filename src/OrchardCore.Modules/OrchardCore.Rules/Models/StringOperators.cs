namespace OrchardCore.Rules.Models
{
    public abstract class StringOperator : ConditionOperator
    {
        public bool CaseSensitive { get; set; }
    }

    public class StringEqualsOperator : StringOperator
    {
    }

    public class StringNotEqualsOperator : StringOperator, INegateOperator
    {
    }

    public class StringStartsWithOperator : StringOperator
    {
    }

    public class StringNotStartsWithOperator : StringOperator, INegateOperator
    {
    }

    public class StringEndsWithOperator : StringOperator
    {
    }

    public class StringNotEndsWithOperator : StringOperator, INegateOperator
    {
    }

    public class StringContainsOperator : StringOperator
    {
    }

    public class StringNotContainsOperator : StringOperator, INegateOperator
    {
    }
}
