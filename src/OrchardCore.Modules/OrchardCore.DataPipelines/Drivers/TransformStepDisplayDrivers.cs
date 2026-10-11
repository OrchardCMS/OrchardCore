using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.DataPipelines.Drivers;

public sealed class FilterStepDisplayDriver : DataPipelineStepDisplayDriver<FilterStepSettings, FilterStepViewModel>
{
    private readonly DataPipelineEditorContext _editorContext;

    public FilterStepDisplayDriver(DataPipelineEditorContext editorContext)
    {
        _editorContext = editorContext;
    }

    protected override string StepName => FilterStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, FilterStepSettings settings, FilterStepViewModel model)
    {
        model.Condition = settings.Condition;
        model.AvailableFields = _editorContext.GetInputFields();

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, FilterStepSettings settings, FilterStepViewModel model, UpdateEditorContext context)
    {
        settings.Condition = model.Condition?.Trim();

        return ValueTask.CompletedTask;
    }
}

public class FilterStepViewModel
{
    public string Condition { get; set; }

    [BindNever]
    public IReadOnlyList<DataField> AvailableFields { get; set; } = [];
}

public sealed class CalculatedFieldsStepDisplayDriver : DataPipelineStepDisplayDriver<CalculatedFieldsStepSettings, CalculatedFieldsStepViewModel>
{
    private readonly DataPipelineEditorContext _editorContext;

    public CalculatedFieldsStepDisplayDriver(DataPipelineEditorContext editorContext)
    {
        _editorContext = editorContext;
    }

    protected override string StepName => CalculatedFieldsStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, CalculatedFieldsStepSettings settings, CalculatedFieldsStepViewModel model)
    {
        model.Fields = settings.Fields;
        model.AvailableFields = _editorContext.GetInputFields();

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, CalculatedFieldsStepSettings settings, CalculatedFieldsStepViewModel model, UpdateEditorContext context)
    {
        settings.Fields = (model.Fields ?? [])
            .Where(field => !string.IsNullOrWhiteSpace(field?.Name) || !string.IsNullOrWhiteSpace(field?.Formula))
            .Select(field => new CalculatedField { Name = field.Name?.Trim(), Formula = field.Formula?.Trim() })
            .ToList();

        return ValueTask.CompletedTask;
    }
}

public class CalculatedFieldsStepViewModel
{
    public List<CalculatedField> Fields { get; set; } = [];

    [BindNever]
    public IReadOnlyList<DataField> AvailableFields { get; set; } = [];
}

public sealed class SelectFieldsStepDisplayDriver : DataPipelineStepDisplayDriver<SelectFieldsStepSettings, SelectFieldsStepViewModel>
{
    private readonly DataPipelineEditorContext _editorContext;

    public SelectFieldsStepDisplayDriver(DataPipelineEditorContext editorContext)
    {
        _editorContext = editorContext;
    }

    protected override string StepName => SelectFieldsStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, SelectFieldsStepSettings settings, SelectFieldsStepViewModel model)
    {
        model.Fields = settings.Fields
            .Select(field => new SelectedFieldRow { Field = field.Field, Name = field.Name, Type = field.Type?.ToString() })
            .ToList();
        model.AvailableFields = _editorContext.GetInputFields();

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, SelectFieldsStepSettings settings, SelectFieldsStepViewModel model, UpdateEditorContext context)
    {
        settings.Fields = (model.Fields ?? [])
            .Where(row => !string.IsNullOrEmpty(row?.Field))
            .Select(row => new SelectedField
            {
                Field = row.Field,
                Name = string.IsNullOrWhiteSpace(row.Name) ? null : row.Name.Trim(),
                Type = Enum.TryParse<DataFieldType>(row.Type, out var type) ? type : null,
            })
            .ToList();

        return ValueTask.CompletedTask;
    }
}

public class SelectFieldsStepViewModel
{
    public List<SelectedFieldRow> Fields { get; set; } = [];

    [BindNever]
    public IReadOnlyList<DataField> AvailableFields { get; set; } = [];
}

public sealed class SelectedFieldRow
{
    public string Field { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }
}

public sealed class SortStepDisplayDriver : DataPipelineStepDisplayDriver<SortStepSettings, SortStepViewModel>
{
    private readonly DataPipelineEditorContext _editorContext;

    public SortStepDisplayDriver(DataPipelineEditorContext editorContext)
    {
        _editorContext = editorContext;
    }

    protected override string StepName => SortStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, SortStepSettings settings, SortStepViewModel model)
    {
        model.Keys = settings.Keys;
        model.AvailableFields = _editorContext.GetInputFields();

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, SortStepSettings settings, SortStepViewModel model, UpdateEditorContext context)
    {
        settings.Keys = (model.Keys ?? []).Where(key => !string.IsNullOrEmpty(key?.Field)).ToList();

        return ValueTask.CompletedTask;
    }
}

public class SortStepViewModel
{
    public List<SortField> Keys { get; set; } = [];

    [BindNever]
    public IReadOnlyList<DataField> AvailableFields { get; set; } = [];
}

public sealed class AggregateStepDisplayDriver : DataPipelineStepDisplayDriver<AggregateStepSettings, AggregateStepViewModel>
{
    private readonly DataPipelineEditorContext _editorContext;

    public AggregateStepDisplayDriver(DataPipelineEditorContext editorContext)
    {
        _editorContext = editorContext;
    }

    protected override string StepName => AggregateStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, AggregateStepSettings settings, AggregateStepViewModel model)
    {
        model.GroupBy = settings.GroupBy.ToArray();
        model.Measures = settings.Measures;
        model.AvailableFields = _editorContext.GetInputFields();

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, AggregateStepSettings settings, AggregateStepViewModel model, UpdateEditorContext context)
    {
        settings.GroupBy = (model.GroupBy ?? []).Where(name => !string.IsNullOrEmpty(name)).ToList();
        settings.Measures = (model.Measures ?? [])
            .Where(measure => !string.IsNullOrWhiteSpace(measure?.Name))
            .Select(measure => new AggregateMeasure
            {
                Name = measure.Name.Trim(),
                Function = measure.Function,
                Field = string.IsNullOrEmpty(measure.Field) ? null : measure.Field,
            })
            .ToList();

        return ValueTask.CompletedTask;
    }
}

public class AggregateStepViewModel
{
    public string[] GroupBy { get; set; } = [];

    public List<AggregateMeasure> Measures { get; set; } = [];

    [BindNever]
    public IReadOnlyList<DataField> AvailableFields { get; set; } = [];
}

public sealed class JoinStepDisplayDriver : DataPipelineStepDisplayDriver<JoinStepSettings, JoinStepViewModel>
{
    private readonly DataPipelineEditorContext _editorContext;

    public JoinStepDisplayDriver(DataPipelineEditorContext editorContext)
    {
        _editorContext = editorContext;
    }

    protected override string StepName => JoinStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, JoinStepSettings settings, JoinStepViewModel model)
    {
        model.JoinType = settings.JoinType;
        model.IgnoreCase = settings.IgnoreCase;
        model.RightPrefix = settings.RightPrefix;
        model.Keys = settings.Keys;
        model.LeftFields = _editorContext.GetInputFields(JoinStep.Left);
        model.RightFields = _editorContext.GetInputFields(JoinStep.Right);

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, JoinStepSettings settings, JoinStepViewModel model, UpdateEditorContext context)
    {
        settings.JoinType = model.JoinType;
        settings.IgnoreCase = model.IgnoreCase;
        settings.RightPrefix = string.IsNullOrWhiteSpace(model.RightPrefix) ? "Right." : model.RightPrefix;
        settings.Keys = (model.Keys ?? [])
            .Where(key => !string.IsNullOrEmpty(key?.LeftField) || !string.IsNullOrEmpty(key?.RightField))
            .ToList();

        return ValueTask.CompletedTask;
    }
}

public class JoinStepViewModel
{
    public DataJoinType JoinType { get; set; }

    public bool IgnoreCase { get; set; }

    public string RightPrefix { get; set; }

    public List<JoinKey> Keys { get; set; } = [];

    [BindNever]
    public IReadOnlyList<DataField> LeftFields { get; set; } = [];

    [BindNever]
    public IReadOnlyList<DataField> RightFields { get; set; } = [];
}

public sealed class UnionStepDisplayDriver : DataPipelineStepDisplayDriver<UnionStepSettings, UnionStepViewModel>
{
    protected override string StepName => UnionStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, UnionStepSettings settings, UnionStepViewModel model)
        => ValueTask.CompletedTask;

    protected override ValueTask UpdateAsync(DataPipelineStep step, UnionStepSettings settings, UnionStepViewModel model, UpdateEditorContext context)
        => ValueTask.CompletedTask;
}

/// <summary>
/// The combine rows step has no settings.
/// </summary>
public sealed class UnionStepSettings;

public class UnionStepViewModel;

public sealed class DistinctStepDisplayDriver : DataPipelineStepDisplayDriver<DistinctStepSettings, DistinctStepViewModel>
{
    private readonly DataPipelineEditorContext _editorContext;

    public DistinctStepDisplayDriver(DataPipelineEditorContext editorContext)
    {
        _editorContext = editorContext;
    }

    protected override string StepName => DistinctStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, DistinctStepSettings settings, DistinctStepViewModel model)
    {
        model.Fields = settings.Fields.ToArray();
        model.AvailableFields = _editorContext.GetInputFields();

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, DistinctStepSettings settings, DistinctStepViewModel model, UpdateEditorContext context)
    {
        settings.Fields = (model.Fields ?? []).Where(name => !string.IsNullOrEmpty(name)).ToList();

        return ValueTask.CompletedTask;
    }
}

public class DistinctStepViewModel
{
    public string[] Fields { get; set; } = [];

    [BindNever]
    public IReadOnlyList<DataField> AvailableFields { get; set; } = [];
}

public sealed class LimitStepDisplayDriver : DataPipelineStepDisplayDriver<LimitStepSettings, LimitStepViewModel>
{
    private readonly IStringLocalizer S;

    public LimitStepDisplayDriver(IStringLocalizer<LimitStepDisplayDriver> localizer)
    {
        S = localizer;
    }

    protected override string StepName => LimitStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, LimitStepSettings settings, LimitStepViewModel model)
    {
        model.Count = settings.Count;
        model.Skip = settings.Skip;

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, LimitStepSettings settings, LimitStepViewModel model, UpdateEditorContext context)
    {
        if (model.Count <= 0)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Count), S["Enter the number of rows to keep."]);
        }

        settings.Count = model.Count;
        settings.Skip = Math.Max(0, model.Skip);

        return ValueTask.CompletedTask;
    }
}

public class LimitStepViewModel
{
    public int Count { get; set; }

    public int Skip { get; set; }
}
