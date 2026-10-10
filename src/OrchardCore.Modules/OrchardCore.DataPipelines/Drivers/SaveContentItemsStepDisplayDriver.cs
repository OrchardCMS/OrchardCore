using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Contents;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.DataPipelines.Drivers;

public sealed class SaveContentItemsStepDisplayDriver : DataPipelineStepDisplayDriver<SaveContentItemsStepSettings, SaveContentItemsStepViewModel>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ContentDataSourceOptions _options;
    private readonly DataPipelineEditorContext _editorContext;
    private readonly IStringLocalizer S;

    public SaveContentItemsStepDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IOptions<ContentDataSourceOptions> options,
        DataPipelineEditorContext editorContext,
        IStringLocalizer<SaveContentItemsStepDisplayDriver> localizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _options = options.Value;
        _editorContext = editorContext;
        S = localizer;
    }

    protected override string StepName => SaveContentItemsStep.StepName;

    protected override async ValueTask EditAsync(DataPipelineStep step, SaveContentItemsStepSettings settings, SaveContentItemsStepViewModel model)
    {
        model.ContentType = settings.ContentType;
        model.Mode = settings.Mode;
        model.MatchBy = settings.MatchBy;
        model.KeyField = settings.KeyField;
        model.Mappings = settings.Mappings;
        model.Publish = settings.Publish;
        model.BatchSize = settings.BatchSize;
        model.AvailableFields = _editorContext.GetInputFields();

        foreach (var definition in (await _contentDefinitionManager.ListTypeDefinitionsAsync()).OrderBy(type => type.DisplayName, StringComparer.CurrentCultureIgnoreCase))
        {
            model.ContentTypes.Add((definition.Name, definition.DisplayName));

            if (definition.Name == settings.ContentType)
            {
                model.ContentTypeName = definition.DisplayName;
                model.Targets = ContentDataColumns.Build(definition, _options, S)
                    .Where(column => column.Write is not null)
                    .Select(column => column.Field)
                    .ToArray();
            }
        }
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, SaveContentItemsStepSettings settings, SaveContentItemsStepViewModel model, UpdateEditorContext context)
    {
        if (model.ContentType != settings.ContentType)
        {
            // Another content type has other parts and fields: start the mappings over.
            settings.ContentType = model.ContentType;
            settings.Mappings = [];
            _editorContext.ReloadEditor = true;
        }
        else
        {
            settings.Mappings = (model.Mappings ?? [])
                .Where(mapping => !string.IsNullOrEmpty(mapping?.Target) || !string.IsNullOrEmpty(mapping?.Source))
                .ToList();
        }

        if (model.BatchSize is < 1 or > 10_000)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.BatchSize), S["Save between 1 and 10,000 items at once."]);
        }

        settings.Mode = model.Mode;
        settings.MatchBy = model.MatchBy;
        settings.KeyField = string.IsNullOrEmpty(model.KeyField) ? null : model.KeyField;
        settings.Publish = model.Publish;
        settings.BatchSize = model.BatchSize;

        return ValueTask.CompletedTask;
    }
}

public class SaveContentItemsStepViewModel
{
    public string ContentType { get; set; }

    public ContentItemSaveMode Mode { get; set; }

    public ContentItemMatch MatchBy { get; set; }

    public string KeyField { get; set; }

    public List<ContentFieldMapping> Mappings { get; set; } = [];

    public bool Publish { get; set; }

    public int BatchSize { get; set; }

    [BindNever]
    public List<(string Name, string DisplayName)> ContentTypes { get; set; } = [];

    [BindNever]
    public string ContentTypeName { get; set; }

    [BindNever]
    public IReadOnlyList<DataField> Targets { get; set; } = [];

    [BindNever]
    public IReadOnlyList<DataField> AvailableFields { get; set; } = [];
}
