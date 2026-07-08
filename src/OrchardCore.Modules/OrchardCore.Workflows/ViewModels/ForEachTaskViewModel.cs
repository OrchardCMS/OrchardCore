using System.ComponentModel.DataAnnotations;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class ForEachTaskViewModel
{
    public string EnumerableExpression { get; set; }

    public string LiquidEnumerableExpression { get; set; }

    public WorkflowScriptSyntax Syntax { get; set; }

    [Required]
    public string LoopVariableName { get; set; }
}
