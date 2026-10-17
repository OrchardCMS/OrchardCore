using System.ComponentModel.DataAnnotations;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class WorkflowTypePropertiesViewModel
{
    public long Id { get; set; }

    [Required]
    public string Name { get; set; }

    public bool IsEnabled { get; set; }
    public bool IsSingleton { get; set; }

    public bool IsSingletonPerCorrelation { get; set; }

    /// <summary>
    /// The choice the forms offer over <see cref="IsSingleton"/> and <see cref="IsSingletonPerCorrelation"/>.
    /// </summary>
    public WorkflowInstanceLimit InstanceLimit
    {
        get => IsSingleton ? WorkflowInstanceLimit.One : IsSingletonPerCorrelation ? WorkflowInstanceLimit.OnePerCorrelation : WorkflowInstanceLimit.Any;
        set
        {
            IsSingleton = value == WorkflowInstanceLimit.One;
            IsSingletonPerCorrelation = value == WorkflowInstanceLimit.OnePerCorrelation;
        }
    }

    public int LockTimeout { get; set; }
    public int LockExpiration { get; set; }
    public bool DeleteFinishedWorkflows { get; set; }

    public bool IsActivity { get; set; }

    public WorkflowBranchingMode BranchingMode { get; set; }

    public bool FaultOnScriptErrors { get; set; }

    public bool RecordActivityData { get; set; }
    public string ReturnUrl { get; set; }
}

/// <summary>
/// How many instances of a workflow run at a time, as the settings forms offer it.
/// </summary>
public enum WorkflowInstanceLimit
{
    /// <summary>
    /// Any number.
    /// </summary>
    Any,

    /// <summary>
    /// One at a time (<see cref="WorkflowType.IsSingleton"/>).
    /// </summary>
    One,

    /// <summary>
    /// One per correlation id (<see cref="WorkflowType.IsSingletonPerCorrelation"/>).
    /// </summary>
    OnePerCorrelation,
}
