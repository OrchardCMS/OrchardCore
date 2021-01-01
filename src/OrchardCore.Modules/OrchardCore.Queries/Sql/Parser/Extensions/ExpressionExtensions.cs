using System;

namespace OrchardCore.Queries.Sql.Parser
{
    public static class ExpressionExtensions
    {
        public static decimal EvaluateAsNumber(this Expression expression)
            => Convert.ToDecimal(expression.Evaluate());

        public static bool EvaluateAsBoolean(this Expression expression)
            => Convert.ToBoolean(expression.Evaluate());
    }
}
