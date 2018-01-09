namespace OrchardCore.Workflows.Models
{
    public class DoubleWorkflowExpression : WorkflowExpression<double>
    {
        public DoubleWorkflowExpression()
        {
        }

        public DoubleWorkflowExpression(string expression) : base(expression)
        {
        }

        public override double Parse()
        {
            double.TryParse(Expression, out var result);
            return result;
        }
    }
}
