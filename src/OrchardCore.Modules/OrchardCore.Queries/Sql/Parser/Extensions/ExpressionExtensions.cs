using System;

namespace OrchardCore.Queries.Sql.Parser
{
    public static class ExpressionExtensions
    {
        public static int EvaluateAsInteger(this Expression expression)
            => Convert.ToInt32(expression.Evaluate());

        public static decimal EvaluateAsDecimal(this Expression expression)
            => Convert.ToDecimal(expression.Evaluate());

        public static bool EvaluateAsBoolean(this Expression expression)
            => Convert.ToBoolean(expression.Evaluate());
    }
}
