using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels;

public class SetVariableTaskViewModel
{
    [Required]
    public string VariableName { get; set; }

    public WorkflowExpressionInput Value { get; set; } = new();
}
