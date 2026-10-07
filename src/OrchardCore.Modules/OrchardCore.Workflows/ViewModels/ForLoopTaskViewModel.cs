namespace OrchardCore.Workflows.ViewModels;

public class ForLoopTaskViewModel
{
    public WorkflowExpressionInput From { get; set; } = new();

    public WorkflowExpressionInput To { get; set; } = new();

    public WorkflowExpressionInput Step { get; set; } = new();

    public string LoopVariableName { get; set; }
}
