using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataPipelines.ViewModels.Designer;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Builds the JSON models of the designer: the toolbox of step types, and the nodes of a pipeline, whose summary is
/// rendered from the <c>Design</c> display of their step.
/// </summary>
public sealed class DataPipelineDesignerModelBuilder
{
    private static readonly DataPipelineStepCategory[] _categories =
    [
        DataPipelineStepCategory.Source,
        DataPipelineStepCategory.Transform,
        DataPipelineStepCategory.File,
        DataPipelineStepCategory.Destination,
    ];

    private readonly IDataPipelineStepTypeManager _stepTypeManager;
    private readonly IDisplayManager<DataPipelineStep> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IDisplayHelper _displayHelper;
    private readonly ViewContextAccessor _viewContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IStringLocalizer S;

    public DataPipelineDesignerModelBuilder(
        IDataPipelineStepTypeManager stepTypeManager,
        IDisplayManager<DataPipelineStep> displayManager,
        IUpdateModelAccessor updateModelAccessor,
        IDisplayHelper displayHelper,
        ViewContextAccessor viewContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        ITempDataProvider tempDataProvider,
        HtmlEncoder htmlEncoder,
        IStringLocalizer<DataPipelineDesignerModelBuilder> localizer)
    {
        _stepTypeManager = stepTypeManager;
        _displayManager = displayManager;
        _updateModelAccessor = updateModelAccessor;
        _displayHelper = displayHelper;
        _viewContextAccessor = viewContextAccessor;
        _httpContextAccessor = httpContextAccessor;
        _tempDataProvider = tempDataProvider;
        _htmlEncoder = htmlEncoder;
        S = localizer;
    }

    /// <summary>
    /// Builds the toolbox: the step types grouped by category, in the order data flows.
    /// </summary>
    /// <returns>The toolbox.</returns>
    public DesignerLibrary BuildLibrary()
    {
        var library = new DesignerLibrary();

        foreach (var category in _categories)
        {
            var steps = _stepTypeManager.GetStepTypes()
                .Where(stepType => stepType.Category == category)
                .OrderBy(stepType => stepType.DisplayName.Value, StringComparer.CurrentCultureIgnoreCase)
                .Select(stepType =>
                {
                    var step = new DataPipelineStep { Type = stepType.Name };

                    return new DesignerLibraryStep
                    {
                        Name = stepType.Name,
                        DisplayText = stepType.DisplayName.Value,
                        Description = stepType.Description.Value,
                        Category = category.ToString(),
                        Icon = stepType.Icon,
                        Inputs = stepType.GetInputs(step).Select(DesignerPort.From).ToList(),
                        Outputs = stepType.GetOutputs(step).Select(DesignerPort.From).ToList(),
                    };
                })
                .ToList();

            if (steps.Count > 0)
            {
                library.Categories.Add(new DesignerLibraryCategory
                {
                    Category = category.ToString(),
                    DisplayName = GetCategoryName(category),
                    Steps = steps,
                });
            }
        }

        return library;
    }

    /// <summary>
    /// Builds the node of a step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>The node.</returns>
    public async Task<DesignerNode> BuildNodeAsync(DataPipelineStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        var stepType = _stepTypeManager.GetStepType(step.Type);

        if (stepType is null)
        {
            return new DesignerNode
            {
                Id = step.StepId,
                Type = step.Type,
                Title = step.Title,
                DisplayText = string.IsNullOrEmpty(step.Title) ? step.Type : step.Title,
                Category = nameof(DataPipelineStepCategory.Transform),
                IsMissing = true,
                X = step.X,
                Y = step.Y,
                DesignHtml = string.Empty,
            };
        }

        var shape = await _displayManager.BuildDisplayAsync(step, _updateModelAccessor.ModelUpdater, "Design");

        return new DesignerNode
        {
            Id = step.StepId,
            Type = step.Type,
            Title = step.Title,
            DisplayText = string.IsNullOrEmpty(step.Title) ? stepType.DisplayName.Value : step.Title,
            Category = stepType.Category.ToString(),
            Icon = stepType.Icon,
            HasEditor = true,
            X = step.X,
            Y = step.Y,
            Inputs = stepType.GetInputs(step).Select(DesignerPort.From).ToList(),
            Outputs = stepType.GetOutputs(step).Select(DesignerPort.From).ToList(),
            DesignHtml = await RenderContentZoneAsync(shape),
        };
    }

    /// <summary>
    /// Builds the nodes of every step of a definition.
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <returns>The nodes.</returns>
    public async Task<List<DesignerNode>> BuildNodesAsync(DataPipelineDefinition definition)
    {
        var nodes = new List<DesignerNode>();

        foreach (var step in definition.Steps)
        {
            nodes.Add(await BuildNodeAsync(step));
        }

        return nodes;
    }

    /// <summary>
    /// Converts a run for the designer.
    /// </summary>
    /// <param name="run">The run.</param>
    /// <param name="url">The page of the run.</param>
    /// <param name="canCancel">Whether the user can cancel it.</param>
    /// <returns>The run model.</returns>
    public static DesignerRun BuildRun(DataPipelineRun run, string url, bool canCancel)
    {
        if (run is null)
        {
            return null;
        }

        var titles = run.Definition?.Steps.ToDictionary(step => step.StepId, step => step.Title ?? step.Type) ?? [];

        return new DesignerRun
        {
            RunId = run.RunId,
            Status = run.Status.ToString(),
            VersionNumber = run.VersionNumber,
            Trigger = run.Trigger,
            TriggeredBy = run.TriggeredBy,
            QueuedUtc = run.QueuedUtc,
            StartedUtc = run.StartedUtc,
            CompletedUtc = run.CompletedUtc,
            Error = run.Error,
            FailedStepId = run.FailedStepId,
            Steps = run.Steps.Select(step => new DesignerRunStep
            {
                StepId = step.StepId,
                Title = string.IsNullOrEmpty(step.Title) ? titles.GetValueOrDefault(step.StepId, step.Type) : step.Title,
                Status = step.Status.ToString(),
                RowsIn = step.RowsIn,
                RowsOut = step.RowsOut,
                FilesIn = step.FilesIn,
                FilesOut = step.FilesOut,
                Warnings = step.Warnings,
                Error = step.Error,
            }).ToList(),
            Deliveries = run.Deliveries.Select(delivery => new DesignerRunDelivery
            {
                StepId = delivery.StepId,
                Description = delivery.Description,
                Url = delivery.Url,
                DeliveredUtc = delivery.DeliveredUtc,
            }).ToList(),
            Log = run.Log.Select(entry => new DesignerRunLogEntry
            {
                Utc = entry.Utc,
                Level = entry.Level.ToString(),
                StepId = entry.StepId,
                Message = entry.Message,
            }).ToList(),
            Url = url,
            CanCancel = canCancel && !run.IsCompleted,
        };
    }

    /// <summary>
    /// Converts a preview for the designer. Values become JSON-friendly: dates as ISO text, everything else as is.
    /// </summary>
    /// <param name="preview">The preview.</param>
    /// <param name="isInputPreview">Whether the preview shows what a destination would receive.</param>
    /// <param name="duration">How long the preview took.</param>
    /// <returns>The preview model.</returns>
    public static DesignerPreview BuildPreview(DataPipelinePreview preview, bool isInputPreview, TimeSpan duration)
        => new()
        {
            StepId = preview.StepId,
            Error = preview.Error,
            IsInputPreview = isInputPreview,
            DurationMilliseconds = (long)duration.TotalMilliseconds,
            Issues = preview.Issues.Select(DesignerIssue.From).ToList(),
            Ports = preview.Ports.Select(port => new DesignerPreviewPort
            {
                Name = port.Name,
                DisplayName = port.DisplayName,
                Kind = port.Kind.ToString(),
                Fields = port.Fields.Select(DesignerField.From).ToList(),
                Rows = port.Rows.Select(row => row.Select(ToJsonValue).ToArray()).ToList(),
                Truncated = port.Truncated,
                Files = port.Files.Select(file => new DesignerPreviewFile
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length,
                    RowCount = file.RowCount,
                }).ToList(),
            }).ToList(),
        };

    private static object ToJsonValue(object value)
        => value switch
        {
            null => null,
            DateTime date => date.TimeOfDay == TimeSpan.Zero && date.Kind != DateTimeKind.Utc
                ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : DateTime.SpecifyKind(date, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture),
            string or bool or long or int or decimal or double => value,
            _ => DataSources.DataValues.ToText(value),
        };

    private string GetCategoryName(DataPipelineStepCategory category)
        => category switch
        {
            DataPipelineStepCategory.Source => S["Sources"],
            DataPipelineStepCategory.Transform => S["Transforms"],
            DataPipelineStepCategory.File => S["Files"],
            _ => S["Destinations"],
        };

    private async Task<string> RenderContentZoneAsync(IShape shape)
    {
        if (shape is not IZoneHolding zoneHolding || !zoneHolding.Zones.IsNotEmpty("Content"))
        {
            return string.Empty;
        }

        // Shape templates are rendered as partial views of the current view. The designer endpoints return JSON, so
        // there is no view, and the first template would be rendered as a main page, wrapped in the theme layout.
        // Render them in a view context without output instead.
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

        public Task RenderAsync(ViewContext context) => Task.CompletedTask;
    }
}
