using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// The workflows usable as an activity (<see cref="WorkflowType.IsActivity"/>), as presets of the Execute Workflow
/// task, listed in the Workflows category.
/// </summary>
public sealed class WorkflowActivityPresetProvider : IActivityPresetProvider
{
    /// <summary>
    /// The prefix of the preset identifiers, followed by the <see cref="WorkflowType.WorkflowTypeId"/>.
    /// </summary>
    public const string IdPrefix = "workflow:";

    private readonly IWorkflowTypeStore _workflowTypeStore;

    internal readonly IStringLocalizer S;

    public WorkflowActivityPresetProvider(IWorkflowTypeStore workflowTypeStore, IStringLocalizer<WorkflowActivityPresetProvider> stringLocalizer)
    {
        _workflowTypeStore = workflowTypeStore;
        S = stringLocalizer;
    }

    public async Task<IEnumerable<ActivityPreset>> GetPresetsAsync()
        => (await _workflowTypeStore.ListAsync())
            .Where(workflowType => workflowType.IsActivity && workflowType.IsEnabled)
            .Select(workflowType => new ActivityPreset
            {
                Id = IdPrefix + workflowType.WorkflowTypeId,
                ActivityName = nameof(ExecuteWorkflowTask),
                DisplayText = workflowType.Name,
                Category = S["Workflows"],
                Description = S["Runs the {0} workflow.", workflowType.Name],
                Properties = new JsonObject
                {
                    [nameof(ExecuteWorkflowTask.WorkflowTypeId)] = workflowType.WorkflowTypeId,

                    // The outputs are known before the task is edited, so they can be bound right away.
                    [nameof(ExecuteWorkflowTask.Outputs)] = JNode.FromObject(workflowType.Variables
                        .Where(variable => variable.IsOutput)
                        .Select(variable => new ExecuteWorkflowOutput
                        {
                            Name = variable.Name,
                            TypeName = variable.TypeName,
                            Description = variable.Description,
                        })
                        .ToList()),
                },
            })
            .ToList();
}
