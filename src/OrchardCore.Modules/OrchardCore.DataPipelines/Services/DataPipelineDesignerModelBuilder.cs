using System.Globalization;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataPipelines.ViewModels.Designer;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Localization;

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

    /// <summary>
    /// The type of the shape of the icon of a step type. The <c>DataPipelineStepIcon__{StepName}</c> alternate gives a
    /// step type its icon.
    /// </summary>
    public const string StepIconShapeType = "DataPipelineStepIcon";

    private readonly IDataPipelineStepTypeManager _stepTypeManager;
    private readonly IDisplayManager<DataPipelineStep> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IShapeHtmlRenderer _shapeHtmlRenderer;
    private readonly IShapeFactory _shapeFactory;
    private readonly IStringLocalizerFactory _localizerFactory;
    private readonly Dictionary<string, string> _icons = new(StringComparer.Ordinal);
    private readonly IStringLocalizer S;

    public DataPipelineDesignerModelBuilder(
        IDataPipelineStepTypeManager stepTypeManager,
        IDisplayManager<DataPipelineStep> displayManager,
        IUpdateModelAccessor updateModelAccessor,
        IShapeHtmlRenderer shapeHtmlRenderer,
        IShapeFactory shapeFactory,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<DataPipelineDesignerModelBuilder> localizer)
    {
        _stepTypeManager = stepTypeManager;
        _displayManager = displayManager;
        _updateModelAccessor = updateModelAccessor;
        _shapeHtmlRenderer = shapeHtmlRenderer;
        _shapeFactory = shapeFactory;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    /// <summary>
    /// Builds the toolbox: the step types grouped by category, in the order data flows.
    /// </summary>
    /// <returns>The toolbox.</returns>
    public async Task<DesignerLibrary> BuildLibraryAsync()
    {
        var library = new DesignerLibrary();

        foreach (var category in _categories)
        {
            var steps = new List<DesignerLibraryStep>();

            foreach (var descriptor in _stepTypeManager.GetStepTypes().Where(descriptor => descriptor.Category == category))
            {
                var step = new DataPipelineStep { Type = descriptor.Name };
                var stepType = _stepTypeManager.GetStepType(descriptor.Name);

                steps.Add(new DesignerLibraryStep
                {
                    Name = descriptor.Name,
                    DisplayText = Localize(descriptor.DisplayName) ?? descriptor.Name,
                    Description = Localize(descriptor.Description),
                    Category = category.ToString(),
                    IconHtml = await RenderIconAsync(descriptor),
                    Inputs = stepType.GetInputs(step).Select(DesignerPort.From).ToList(),
                    Outputs = stepType.GetOutputs(step).Select(DesignerPort.From).ToList(),
                });
            }

            steps.Sort((left, right) => StringComparer.CurrentCultureIgnoreCase.Compare(left.DisplayText, right.DisplayText));

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

        var descriptor = _stepTypeManager.GetDescriptor(step.Type);
        var stepType = descriptor is null ? null : _stepTypeManager.GetStepType(step.Type);

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
            DisplayText = string.IsNullOrEmpty(step.Title) ? Localize(descriptor.DisplayName) ?? step.Type : step.Title,
            Category = descriptor.Category.ToString(),
            IconHtml = await RenderIconAsync(descriptor),
            HasEditor = true,
            X = step.X,
            Y = step.Y,
            Inputs = stepType.GetInputs(step).Select(DesignerPort.From).ToList(),
            Outputs = stepType.GetOutputs(step).Select(DesignerPort.From).ToList(),
            DesignHtml = await _shapeHtmlRenderer.RenderZoneAsync(shape, "Content"),
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
    {
        return new()
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
    }

    private static object ToJsonValue(object value)
    {
        return value switch
        {
            null => null,
            // A date and time of no kind is a local time, such as one converted to a time zone: it has no "Z".
            DateTime date => DataSources.Files.DataFileValues.ToText(
                date,
                date.TimeOfDay == TimeSpan.Zero && date.Kind != DateTimeKind.Utc ? DataSources.DataFieldType.Date : DataSources.DataFieldType.DateTime),
            string or bool or long or int or decimal or double => value,
            _ => DataSources.DataValues.ToText(value),
        };
    }

    // The icon of a step type is a shape, so a template gives a step type its icon: DataPipelineStepIcon-{Name}.cshtml.
    private async Task<string> RenderIconAsync(DataPipelineStepTypeDescriptor descriptor)
    {
        if (_icons.TryGetValue(descriptor.Name, out var html))
        {
            return html;
        }

        var shape = await _shapeFactory.CreateAsync(StepIconShapeType, Arguments.From(new
        {
            StepType = descriptor.Name,
            descriptor.Category,
        }));

        shape.Metadata.Alternates.Add($"{StepIconShapeType}__{descriptor.Name}");

        html = await _shapeHtmlRenderer.RenderAsync(shape);
        _icons[descriptor.Name] = html;

        return html;
    }

    private string Localize(LocalizationSource source)
    {
        return source is null ? null : _localizerFactory.Localize(source)?.Value;
    }

    private string GetCategoryName(DataPipelineStepCategory category)
    {
        return category switch
        {
            DataPipelineStepCategory.Source => S["Sources"],
            DataPipelineStepCategory.Transform => S["Transforms"],
            DataPipelineStepCategory.File => S["Files"],
            _ => S["Destinations"],
        };
    }
}
