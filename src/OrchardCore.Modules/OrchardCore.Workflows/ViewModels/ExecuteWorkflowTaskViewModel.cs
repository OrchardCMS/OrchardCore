using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class ExecuteWorkflowTaskViewModel
{
    public string WorkflowTypeId { get; set; }

    public bool WaitForCompletion { get; set; } = true;

    /// <summary>
    /// The input variables of the selected workflow, with their expressions.
    /// </summary>
    public List<ExecuteWorkflowInputViewModel> Inputs { get; set; } = [];

    /// <summary>
    /// The workflows usable as an activity.
    /// </summary>
    [BindNever]
    public IList<WorkflowType> Workflows { get; set; } = [];
}

public class ExecuteWorkflowInputViewModel
{
    /// <summary>
    /// The name of the input variable.
    /// </summary>
    public string Name { get; set; }

    public WorkflowExpressionInput Value { get; set; } = new();

    [BindNever]
    public string TypeName { get; set; }

    [BindNever]
    public string Description { get; set; }
}
