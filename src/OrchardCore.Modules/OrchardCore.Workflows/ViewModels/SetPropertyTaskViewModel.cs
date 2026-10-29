using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels;

public class SetPropertyTaskViewModel
{
    [Required]
    public string PropertyName { get; set; }

    public WorkflowExpressionInput Value { get; set; } = new();
}
