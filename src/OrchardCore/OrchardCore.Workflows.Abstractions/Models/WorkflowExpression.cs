namespace OrchardCore.Workflows.Models
{
    public class WorkflowExpression<T>
    {
        public WorkflowExpression()
        {
        }

        public WorkflowExpression(string expression)
        {
            Expression = expression;
        }

        public string Expression { get; set; }

        public override string ToString()
        {
            return Expression;
        }
    }
}
