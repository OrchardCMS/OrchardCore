namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents an expression that is evaluated at workflow runtime.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the expression.</typeparam>
    public class WorkflowExpression<T>
    {
        /// <summary>
        /// The expression.
        /// </summary>
        public string Expression { get; set; }

        public override string ToString()
        {
            return Expression;
        }
    }
}
