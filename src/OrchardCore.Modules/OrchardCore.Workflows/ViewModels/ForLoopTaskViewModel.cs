using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class ForLoopTaskViewModel
{
    public string FromExpression { get; set; }
    public string LiquidFromExpression { get; set; }
    public string ToExpression { get; set; }
    public string LiquidToExpression { get; set; }
    public string StepExpression { get; set; }
    public string LiquidStepExpression { get; set; }
    public string LoopVariableName { get; set; }
    public WorkflowScriptSyntax Syntax { get; set; }
}
