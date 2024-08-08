using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class CorrelateTaskViewModel
{
    public string Value { get; set; }

    public WorkflowScriptSyntax Syntax { get; set; }
}
