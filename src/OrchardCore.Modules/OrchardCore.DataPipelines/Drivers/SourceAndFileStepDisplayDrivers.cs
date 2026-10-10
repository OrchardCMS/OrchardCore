using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.DataPipelines.Drivers;

public sealed class DataSourceStepDisplayDriver : DataPipelineStepDisplayDriver<DataSourceStepSettings, DataSourceStepViewModel>
{
    private const string Separator = "::";

    private readonly IDataSourceManager _dataSourceManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DataPipelineEditorContext _editorContext;
    private readonly IStringLocalizer S;

    public DataSourceStepDisplayDriver(
        IDataSourceManager dataSourceManager,
        IHttpContextAccessor httpContextAccessor,
        DataPipelineEditorContext editorContext,
        IStringLocalizer<DataSourceStepDisplayDriver> localizer)
    {
        _dataSourceManager = dataSourceManager;
        _httpContextAccessor = httpContextAccessor;
        _editorContext = editorContext;
        S = localizer;
    }

    protected override string StepName => DataSourceStep.StepName;

    protected override async ValueTask EditAsync(DataPipelineStep step, DataSourceStepSettings settings, DataSourceStepViewModel model)
    {
        model.DataSet = string.IsNullOrEmpty(settings.Source) ? null : settings.Source + Separator + settings.DataSet;
        model.Fields = settings.Fields.ToArray();
        model.Filters = settings.Filters
            .Select(filter => new DataSourceFilterRow
            {
                Field = filter.Field,
                Operator = filter.Operator,
                Value = filter.Values.Count > 0 ? filter.Values[0] : null,
                SecondValue = filter.Values.Count > 1 ? filter.Values[1] : null,
            })
            .ToList();
        model.MaxRows = settings.MaxRows;
        model.Parameters = string.Join(System.Environment.NewLine, settings.Parameters.Select(pair => $"{pair.Key}={pair.Value}"));

        var context = new DataSourceContext { User = _httpContextAccessor.HttpContext?.User };

        foreach (var dataSource in _dataSourceManager.GetDataSources())
        {
            foreach (var dataSet in await dataSource.GetDataSetsAsync(context))
            {
                model.DataSets.Add(new DataSetOption
                {
                    Value = dataSource.Name + Separator + dataSet.Name,
                    Text = dataSet.DisplayName,
                    Group = dataSource.DisplayName.Value,
                });
            }
        }

        model.SourceName = _dataSourceManager.GetDataSource(settings.Source)?.DisplayName.Value;
        model.DataSetName = model.DataSets.FirstOrDefault(option => option.Value == model.DataSet)?.Text ?? settings.DataSet;

        if (!string.IsNullOrEmpty(settings.Source) && _dataSourceManager.GetDataSource(settings.Source) is { } source)
        {
            model.AvailableFields = (await source.GetSchemaAsync(settings.DataSet, context))?.Fields.ToArray() ?? [];
        }
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, DataSourceStepSettings settings, DataSourceStepViewModel model, UpdateEditorContext context)
    {
        var parts = (model.DataSet ?? string.Empty).Split(Separator, 2);
        var source = parts.Length == 2 ? parts[0] : null;
        var dataSet = parts.Length == 2 ? parts[1] : null;

        if (source != settings.Source || dataSet != settings.DataSet)
        {
            // Another data set has other fields: start over, and render the editor again with its fields.
            settings.Source = source;
            settings.DataSet = dataSet;
            settings.Fields = [];
            settings.Filters = [];
            _editorContext.ReloadEditor = true;
        }
        else
        {
            settings.Fields = (model.Fields ?? []).Where(name => !string.IsNullOrEmpty(name)).ToList();
            settings.Filters = (model.Filters ?? [])
                .Where(filter => !string.IsNullOrEmpty(filter?.Field))
                .Select(filter => new DataSourceFilter
                {
                    Field = filter.Field,
                    Operator = filter.Operator,
                    Values = filter.Operator == DataFilterOperator.Between
                        ? [filter.Value ?? string.Empty, filter.SecondValue ?? string.Empty]
                        : string.IsNullOrEmpty(filter.Value) ? [] : [filter.Value],
                })
                .ToList();
        }

        if (model.MaxRows < 0)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.MaxRows), S["The number of rows can't be negative."]);
        }

        settings.MaxRows = Math.Max(0, model.MaxRows);
        settings.Parameters = ParseParameters(model.Parameters);

        return ValueTask.CompletedTask;
    }

    private static Dictionary<string, string> ParseParameters(string text)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in (text ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = line.IndexOf('=', StringComparison.Ordinal);

            if (index > 0)
            {
                parameters[line[..index].Trim()] = line[(index + 1)..].Trim();
            }
        }

        return parameters;
    }
}

public class DataSourceStepViewModel
{
    public string DataSet { get; set; }

    public string[] Fields { get; set; } = [];

    public List<DataSourceFilterRow> Filters { get; set; } = [];

    public int MaxRows { get; set; }

    public string Parameters { get; set; }

    [BindNever]
    public List<DataSetOption> DataSets { get; set; } = [];

    [BindNever]
    public IReadOnlyList<DataField> AvailableFields { get; set; } = [];

    [BindNever]
    public string SourceName { get; set; }

    [BindNever]
    public string DataSetName { get; set; }
}

public sealed class DataSourceFilterRow
{
    public string Field { get; set; }

    public DataFilterOperator Operator { get; set; }

    public string Value { get; set; }

    public string SecondValue { get; set; }
}

public sealed class DataSetOption
{
    public string Value { get; set; }

    public string Text { get; set; }

    public string Group { get; set; }
}

public sealed class CreateFileStepDisplayDriver : DataPipelineStepDisplayDriver<CreateFileStepSettings, CreateFileStepViewModel>
{
    private readonly IDataFileFormatManager _formatManager;
    private readonly IStringLocalizer S;

    public CreateFileStepDisplayDriver(IDataFileFormatManager formatManager, IStringLocalizer<CreateFileStepDisplayDriver> localizer)
    {
        _formatManager = formatManager;
        S = localizer;
    }

    protected override string StepName => CreateFileStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, CreateFileStepSettings settings, CreateFileStepViewModel model)
    {
        model.Format = settings.Format;
        model.FileName = settings.FileName;
        model.IncludeHeader = settings.IncludeHeader;
        model.UseDisplayNames = settings.UseDisplayNames;
        model.Delimiter = settings.Delimiter;
        model.SheetName = settings.SheetName;
        model.Indented = settings.Indented;
        model.Formats = _formatManager.GetFormats();
        model.FormatName = _formatManager.GetFormat(settings.Format)?.DisplayName.Value ?? settings.Format;

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, CreateFileStepSettings settings, CreateFileStepViewModel model, UpdateEditorContext context)
    {
        if (_formatManager.GetFormat(model.Format) is null)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Format), S["Choose the format of the file."]);
        }

        settings.Format = model.Format;
        settings.FileName = model.FileName?.Trim();
        settings.IncludeHeader = model.IncludeHeader;
        settings.UseDisplayNames = model.UseDisplayNames;
        settings.Delimiter = string.IsNullOrEmpty(model.Delimiter) ? "," : model.Delimiter;
        settings.SheetName = string.IsNullOrWhiteSpace(model.SheetName) ? null : model.SheetName.Trim();
        settings.Indented = model.Indented;

        return ValueTask.CompletedTask;
    }
}

public class CreateFileStepViewModel
{
    public string Format { get; set; }

    public string FileName { get; set; }

    public bool IncludeHeader { get; set; }

    public bool UseDisplayNames { get; set; }

    public string Delimiter { get; set; }

    public string SheetName { get; set; }

    public bool Indented { get; set; }

    [BindNever]
    public IReadOnlyList<IDataFileFormat> Formats { get; set; } = [];

    [BindNever]
    public string FormatName { get; set; }
}

public sealed class ZipFilesStepDisplayDriver : DataPipelineStepDisplayDriver<ZipFilesStepSettings, ZipFilesStepViewModel>
{
    protected override string StepName => ZipFilesStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, ZipFilesStepSettings settings, ZipFilesStepViewModel model)
    {
        model.FileName = settings.FileName;

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, ZipFilesStepSettings settings, ZipFilesStepViewModel model, UpdateEditorContext context)
    {
        settings.FileName = model.FileName?.Trim();

        return ValueTask.CompletedTask;
    }
}

public class ZipFilesStepViewModel
{
    public string FileName { get; set; }
}
