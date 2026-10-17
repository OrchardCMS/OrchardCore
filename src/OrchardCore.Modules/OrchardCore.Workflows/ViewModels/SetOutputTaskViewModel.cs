using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels;

public class SetOutputTaskViewModel
{
    [Required]
    public string OutputName { get; set; }

    public WorkflowExpressionInput Value { get; set; } = new();
}
