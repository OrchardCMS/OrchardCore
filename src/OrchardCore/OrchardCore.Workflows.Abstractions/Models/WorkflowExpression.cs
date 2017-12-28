namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents an expression that is evaluated at workflow runtime.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the expression.</typeparam>
    public class WorkflowExpression<T>
    {
        public WorkflowExpression()
        {
        }

        public WorkflowExpression(string expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// The expression.
        /// </summary>
        public string Expression { get; set; } // Needs a setter for deserialization purposes.

        public override string ToString()
        {
            return Expression;
        }
    }
}
