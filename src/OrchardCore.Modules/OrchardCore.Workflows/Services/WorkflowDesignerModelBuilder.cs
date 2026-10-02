using System.Text.Encodings.Web;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Entities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Builds the designer models of a workflow type: its activities with their outcomes and rendered
/// <c>{Name}_Fields_Design</c> shapes, and the toolbox with the rendered <c>{Name}_Fields_Thumbnail</c> shapes.
/// </summary>
public sealed class WorkflowDesignerModelBuilder
{
    private readonly IWorkflowManager _workflowManager;
    private readonly IActivityLibrary _activityLibrary;
    private readonly IActivityDisplayManager _activityDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IDisplayHelper _displayHelper;
    private readonly HtmlEncoder _htmlEncoder;

    public WorkflowDesignerModelBuilder(
        IWorkflowManager workflowManager,
        IActivityLibrary activityLibrary,
        IActivityDisplayManager activityDisplayManager,
        IUpdateModelAccessor updateModelAccessor,
        IDisplayHelper displayHelper,
        HtmlEncoder htmlEncoder)
    {
        _workflowManager = workflowManager;
        _activityLibrary = activityLibrary;
        _activityDisplayManager = activityDisplayManager;
        _updateModelAccessor = updateModelAccessor;
        _displayHelper = displayHelper;
        _htmlEncoder = htmlEncoder;
    }

    /// <summary>
    /// Returns the outcomes an activity can produce, given its current properties.
    /// </summary>
    public static async Task<IReadOnlyList<Outcome>> GetOutcomesAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        ArgumentNullException.ThrowIfNull(workflowContext);
        ArgumentNullException.ThrowIfNull(activityContext);

        return (await activityContext.Activity.GetPossibleOutcomesAsync(workflowContext, activityContext)).ToArray();
    }

    /// <summary>
    /// Builds the designer nodes of every activity of a workflow type, which can be a transient type built
    /// from a draft.
    /// </summary>
    public async Task<IReadOnlyList<WorkflowDesignerNode>> BuildNodesAsync(WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        using var workflowContext = await CreateWorkflowContextAsync(workflowType);
        var nodes = new List<WorkflowDesignerNode>(workflowType.Activities.Count);

        foreach (var record in workflowType.Activities)
        {
            nodes.Add(await BuildNodeAsync(workflowContext, workflowContext.GetActivity(record.ActivityId)));
        }

        return nodes;
    }

    /// <summary>
    /// Builds the designer node of one activity of a workflow type.
    /// </summary>
    public async Task<WorkflowDesignerNode> BuildNodeAsync(WorkflowType workflowType, string activityId)
    {
        ArgumentNullException.ThrowIfNull(workflowType);
        ArgumentException.ThrowIfNullOrEmpty(activityId);

        using var workflowContext = await CreateWorkflowContextAsync(workflowType);

        return await BuildNodeAsync(workflowContext, workflowContext.GetActivity(activityId));
    }

    /// <summary>
    /// Builds the toolbox: every registered activity, grouped by category.
    /// </summary>
    public async Task<WorkflowDesignerLibrary> BuildLibraryAsync()
    {
        var categories = new List<WorkflowDesignerCategory>();
        var activities = _activityLibrary.ListActivities().ToList();

        foreach (var category in _activityLibrary.ListCategories())
        {
            var descriptors = new List<WorkflowDesignerActivityDescriptor>();

            foreach (var activity in activities
                .Where(activity => activity.Category.Name == category.Name)
                .OrderBy(activity => activity.DisplayText.Value, StringComparer.CurrentCultureIgnoreCase))
            {
                var shape = await _activityDisplayManager.BuildDisplayAsync(activity, _updateModelAccessor.ModelUpdater, "Thumbnail");

                descriptors.Add(new WorkflowDesignerActivityDescriptor
                {
                    Name = activity.Name,
                    DisplayText = activity.DisplayText.Value,
                    Category = activity.Category.Value,
                    IsEvent = activity.IsEvent(),
                    HasEditor = activity.HasEditor,
                    ThumbnailHtml = await RenderContentZoneAsync(shape),
                });
            }

            if (descriptors.Count > 0)
            {
                categories.Add(new WorkflowDesignerCategory
                {
                    Name = category.Value,
                    Activities = descriptors,
                });
            }
        }

        return new WorkflowDesignerLibrary { Categories = categories };
    }

    private async Task<WorkflowExecutionContext> CreateWorkflowContextAsync(WorkflowType workflowType)
    {
        var workflow = _workflowManager.NewWorkflow(workflowType);

        return await _workflowManager.CreateWorkflowExecutionContextAsync(workflowType, workflow);
    }

    private async Task<WorkflowDesignerNode> BuildNodeAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var record = activityContext.ActivityRecord;
        var activity = activityContext.Activity;
        var outcomes = await GetOutcomesAsync(workflowContext, activityContext);
        var shape = await _activityDisplayManager.BuildDisplayAsync(activity, _updateModelAccessor.ModelUpdater, "Design");
        var title = activity.TryGet<ActivityMetadata>(out var metadata) && !string.IsNullOrWhiteSpace(metadata.Title)
            ? metadata.Title
            : activity.DisplayText.Value;

        return new WorkflowDesignerNode
        {
            Id = record.ActivityId,
            Name = record.Name,
            X = record.X,
            Y = record.Y,
            IsStart = record.IsStart,
            IsEvent = activity.IsEvent(),
            HasEditor = activity.HasEditor,
            IsMissing = activity is MissingActivity,
            Title = title,
            DisplayText = activity.DisplayText.Value,
            Category = activity.Category.Value,
            DesignHtml = await RenderContentZoneAsync(shape),
            Outcomes = outcomes
                .Select(outcome => new WorkflowDesignerOutcome
                {
                    Name = outcome.Name,
                    DisplayName = outcome.DisplayName?.Value ?? outcome.Name,
                })
                .ToList(),
        };
    }

    // The 'Activity_Design' and 'Activity_Thumbnail' templates wrap the activity's own shape with the markup of
    // the legacy canvas and picker. The designer draws its own node and card, so it only renders the 'Content'
    // zone, where the drivers place the '{Name}_Fields_Design' and '{Name}_Fields_Thumbnail' shapes.
    private async Task<string> RenderContentZoneAsync(IShape shape)
    {
        if (shape is not IZoneHolding zoneHolding || !zoneHolding.Zones.IsNotEmpty("Content"))
        {
            return string.Empty;
        }

        var content = await _displayHelper.ShapeExecuteAsync(zoneHolding.Zones["Content"]);

        using var writer = new StringWriter();
        content.WriteTo(writer, _htmlEncoder);

        return writer.ToString();
    }
}
