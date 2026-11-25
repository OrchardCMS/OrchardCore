using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class WorkflowTypePropertiesViewModel : IWorkflowTypeExecutionSettings
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

    /// <summary>
    /// Gets or sets the page editing the properties, which adapts their editor to it.
    /// </summary>
    [BindNever]
    public WorkflowTypePropertiesPage Page { get; set; }

    /// <summary>
    /// Gets or sets the editor of the properties, built by the display drivers of <see cref="WorkflowTypePropertiesViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}

/// <summary>
/// The page editing the properties of a workflow type.
/// </summary>
public enum WorkflowTypePropertiesPage
{
    /// <summary>
    /// The page creating a workflow type.
    /// </summary>
    Create,

    /// <summary>
    /// The page cloning a workflow type.
    /// </summary>
    Clone,

    /// <summary>
    /// The Workflow tab of the designer.
    /// </summary>
    Designer,
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
