using System.ComponentModel.DataAnnotations;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class SetPropertyTaskViewModel
{
    [Required]
    public string PropertyName { get; set; }

    public string Value { get; set; }

    public string LiquidValue { get; set; }

    public WorkflowScriptSyntax Syntax { get; set; }
}
