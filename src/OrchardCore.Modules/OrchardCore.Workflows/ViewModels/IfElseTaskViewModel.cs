using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class IfElseTaskViewModel
{
    public string ConditionExpression { get; set; }

    public string LiquidConditionExpression { get; set; }

    public WorkflowScriptSyntax Syntax { get; set; }
}
