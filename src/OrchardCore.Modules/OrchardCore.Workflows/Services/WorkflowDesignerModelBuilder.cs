using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Entities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Options;
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
    private readonly ViewContextAccessor _viewContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly WorkflowOptions _workflowOptions;
    private readonly IEnumerable<IActivityPresetProvider> _presetProviders;
    private readonly IWorkflowTypeStore _workflowTypeStore;

    public WorkflowDesignerModelBuilder(
        IWorkflowManager workflowManager,
        IActivityLibrary activityLibrary,
        IActivityDisplayManager activityDisplayManager,
        IUpdateModelAccessor updateModelAccessor,
        IDisplayHelper displayHelper,
        ViewContextAccessor viewContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        ITempDataProvider tempDataProvider,
        HtmlEncoder htmlEncoder,
        IOptions<WorkflowOptions> workflowOptions,
        IEnumerable<IActivityPresetProvider> presetProviders,
        IWorkflowTypeStore workflowTypeStore)
    {
        _workflowManager = workflowManager;
        _activityLibrary = activityLibrary;
        _activityDisplayManager = activityDisplayManager;
        _updateModelAccessor = updateModelAccessor;
        _displayHelper = displayHelper;
        _viewContextAccessor = viewContextAccessor;
        _httpContextAccessor = httpContextAccessor;
        _tempDataProvider = tempDataProvider;
        _htmlEncoder = htmlEncoder;
        _workflowOptions = workflowOptions.Value;
        _presetProviders = presetProviders;
        _workflowTypeStore = workflowTypeStore;
    }

    /// <summary>
    /// Returns the preset of an identifier whose activity is available, or <see langword="null"/>.
    /// </summary>
    public async Task<ActivityPreset> FindPresetAsync(string id)
    {
        var preset = await _presetProviders.FindPresetAsync(id);

        return preset is not null && _activityLibrary.GetActivityByName(preset.ActivityName) is not null ? preset : null;
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
                    Icon = WorkflowDesignerIcons.Resolve(activity, _workflowOptions),
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

        await AddPresetsAsync(categories, activities);

        return new WorkflowDesignerLibrary { Categories = categories };
    }

    // The presets are listed after the activities of their category, or in a category of their own.
    private async Task AddPresetsAsync(List<WorkflowDesignerCategory> categories, List<IActivity> activities)
    {
        var descriptors = new List<WorkflowDesignerActivityDescriptor>();

        foreach (var preset in await _presetProviders.ListPresetsAsync())
        {
            var activity = activities.FirstOrDefault(activity => activity.Name == preset.ActivityName);

            if (activity is null || string.IsNullOrEmpty(preset.Id))
            {
                continue;
            }

            var icon = string.IsNullOrEmpty(preset.Icon) ? WorkflowDesignerIcons.Resolve(activity, _workflowOptions) : preset.Icon;

            descriptors.Add(new WorkflowDesignerActivityDescriptor
            {
                Name = activity.Name,
                Preset = preset.Id,
                DisplayText = preset.DisplayText,
                Category = string.IsNullOrEmpty(preset.Category) ? activity.Category.Value : preset.Category,
                IsEvent = activity.IsEvent(),
                HasEditor = activity.HasEditor,
                ThumbnailHtml = PresetThumbnail(preset, icon),
                Icon = icon,
            });
        }

        foreach (var group in descriptors.GroupBy(descriptor => descriptor.Category))
        {
            var presets = group.OrderBy(descriptor => descriptor.DisplayText, StringComparer.CurrentCultureIgnoreCase);
            var index = categories.FindIndex(category => category.Name == group.Key);

            if (index < 0)
            {
                categories.Add(new WorkflowDesignerCategory { Name = group.Key, Activities = presets.ToList() });
            }
            else
            {
                categories[index] = new WorkflowDesignerCategory { Name = group.Key, Activities = categories[index].Activities.Concat(presets).ToList() };
            }
        }
    }

    private string PresetThumbnail(ActivityPreset preset, string icon)
    {
        var html = $"<h4 class=\"card-title\"><i class=\"{_htmlEncoder.Encode(icon ?? string.Empty)}\" aria-hidden=\"true\"></i>{_htmlEncoder.Encode(preset.DisplayText ?? string.Empty)}</h4>";

        return string.IsNullOrEmpty(preset.Description) ? html : $"{html}<p>{_htmlEncoder.Encode(preset.Description)}</p>";
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
        var displayText = await GetDisplayTextAsync(activity);
        var title = activity.TryGet<ActivityMetadata>(out var metadata) && !string.IsNullOrWhiteSpace(metadata.Title)
            ? metadata.Title
            : displayText;

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
            DisplayText = displayText,
            Category = activity.Category.Value,
            DesignHtml = await RenderContentZoneAsync(shape),
            Icon = WorkflowDesignerIcons.Resolve(activity, _workflowOptions),
            Outcomes = outcomes
                .Select(outcome => new WorkflowDesignerOutcome
                {
                    Name = outcome.Name,
                    DisplayName = outcome.DisplayName?.Value ?? outcome.Name,
                })
                .ToList(),
            Outputs = activity is IActivityOutputs activityOutputs
                ? activityOutputs.GetOutputs()
                    .Select(output => new WorkflowDesignerOutput
                    {
                        Name = output.Name,
                        TypeName = output.TypeName,
                        DisplayName = output.DisplayName?.Value ?? output.Name,
                    })
                    .ToList()
                : [],
            OutputBindings = record.Properties.GetOutputBindings(),
            ProvidedValues = activity.GetProvidedValues(_workflowOptions)
                .Select(value => new WorkflowDesignerProvidedValue
                {
                    Source = value.Source.ToString(),
                    Name = value.Name,
                    TypeName = value.TypeName,
                    Description = string.IsNullOrEmpty(value.Description?.Value) ? null : value.Description.Value,
                    Members = (value.Members ?? [])
                        .Select(member => new WorkflowDesignerProvidedValue
                        {
                            Name = member.Name,
                            TypeName = member.TypeName,
                            Description = string.IsNullOrEmpty(member.Description?.Value) ? null : member.Description.Value,
                        })
                        .ToList(),
                    AvailableToItself = value.AvailableToItself,
                })
                .ToList(),
        };
    }

    // A task that runs a workflow usable as an activity is shown as that workflow, as the activities pane lists it.
    private async Task<string> GetDisplayTextAsync(IActivity activity)
    {
        if (activity is ExecuteWorkflowTask executeWorkflow && !string.IsNullOrEmpty(executeWorkflow.WorkflowTypeId))
        {
            var workflowType = await _workflowTypeStore.GetAsync(executeWorkflow.WorkflowTypeId);

            if (!string.IsNullOrWhiteSpace(workflowType?.Name))
            {
                return workflowType.Name;
            }
        }

        return activity.DisplayText.Value;
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

        // Shape templates are rendered as partial views of the current view. The designer endpoints return JSON,
        // so there is no view, and the first template of the request would be rendered as a main page instead,
        // wrapped in the theme layout. Render them in a view context without output, as Liquid templates do.
        var previousViewContext = _viewContextAccessor.ViewContext;

        if (previousViewContext?.View is null && _httpContextAccessor.HttpContext is { } httpContext)
        {
            var actionContext = await httpContext.GetActionContextAsync();

            _viewContextAccessor.ViewContext = new ViewContext(
                actionContext,
                NullView.Instance,
                new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                new TempDataDictionary(httpContext, _tempDataProvider),
                TextWriter.Null,
                new HtmlHelperOptions());
        }

        try
        {
            var content = await _displayHelper.ShapeExecuteAsync(zoneHolding.Zones["Content"]);

            using var writer = new StringWriter();
            content.WriteTo(writer, _htmlEncoder);

            return writer.ToString();
        }
        finally
        {
            _viewContextAccessor.ViewContext = previousViewContext;
        }
    }

    private sealed class NullView : IView
    {
        public static readonly NullView Instance = new();

        public string Path => string.Empty;

        public Task RenderAsync(ViewContext context)
            => Task.CompletedTask;
    }
}
