namespace OrchardCore.Rules.Models
{
    public abstract class StringOperator : ConditionOperator
    {
        public bool CaseSensitive { get; set; }
    }

    public class StringEqualsOperator : StringOperator
    {
    }

    public class StringNotEqualsOperator : StringOperator
    {
    }    

    public class StringStartsWithOperator : StringOperator
    {
    }

    public class StringNotStartsWithOperator : StringOperator
    {
    }   

    public class StringEndsWithOperator : StringOperator
    {
    }

    public class StringNotEndsWithOperator : StringOperator
    {
    }   
     
    public class StringContainsOperator : StringOperator
    {
    }

    public class StringNotContainsOperator : StringOperator
    {
    }    
}