using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels;

public class ForEachTaskViewModel
{
    public WorkflowExpressionInput Enumerable { get; set; } = new();

    [Required]
    public string LoopVariableName { get; set; }
}
