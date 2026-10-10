using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources.Files;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.DataPipelines.Drivers;

public sealed class SaveToMediaStepDisplayDriver : DataPipelineStepDisplayDriver<SaveToMediaStepSettings, SaveToMediaStepViewModel>
{
    protected override string StepName => SaveToMediaStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, SaveToMediaStepSettings settings, SaveToMediaStepViewModel model)
    {
        model.Folder = settings.Folder;
        model.Overwrite = settings.Overwrite;

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, SaveToMediaStepSettings settings, SaveToMediaStepViewModel model, UpdateEditorContext context)
    {
        settings.Folder = model.Folder?.Trim();
        settings.Overwrite = model.Overwrite;

        return ValueTask.CompletedTask;
    }
}

public class SaveToMediaStepViewModel
{
    public string Folder { get; set; }

    public bool Overwrite { get; set; }
}

public sealed class ReadMediaFileStepDisplayDriver : DataPipelineStepDisplayDriver<ReadMediaFileStepSettings, ReadMediaFileStepViewModel>
{
    private readonly IDataFileFormatManager _formatManager;

    public ReadMediaFileStepDisplayDriver(IDataFileFormatManager formatManager)
    {
        _formatManager = formatManager;
    }

    protected override string StepName => ReadMediaFileStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, ReadMediaFileStepSettings settings, ReadMediaFileStepViewModel model)
    {
        model.Path = settings.Path;
        model.Format = settings.Format;
        model.Delimiter = settings.Delimiter;
        model.HasHeaderRow = settings.HasHeaderRow;
        model.SheetName = settings.SheetName;
        model.Formats = _formatManager.GetFormats();

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, ReadMediaFileStepSettings settings, ReadMediaFileStepViewModel model, UpdateEditorContext context)
    {
        settings.Path = model.Path?.Trim();
        settings.Format = string.IsNullOrEmpty(model.Format) ? null : model.Format;
        settings.Delimiter = string.IsNullOrEmpty(model.Delimiter) ? "," : model.Delimiter;
        settings.HasHeaderRow = model.HasHeaderRow;
        settings.SheetName = string.IsNullOrWhiteSpace(model.SheetName) ? null : model.SheetName.Trim();

        return ValueTask.CompletedTask;
    }
}

public class ReadMediaFileStepViewModel
{
    public string Path { get; set; }

    public string Format { get; set; }

    public string Delimiter { get; set; }

    public bool HasHeaderRow { get; set; }

    public string SheetName { get; set; }

    [BindNever]
    public IReadOnlyList<IDataFileFormat> Formats { get; set; } = [];
}
