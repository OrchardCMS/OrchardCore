using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.Services;
using OrchardCore.DataOrchestrator.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class ExcelSourceDisplayDriver : EtlActivityDisplayDriver<ExcelSource, ExcelSourceViewModel>
{
    private readonly IOptions<ShellOptions> _shellOptions;
    private readonly ShellSettings _shellSettings;
    private readonly IStringLocalizer S;

    public ExcelSourceDisplayDriver(
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        IStringLocalizer<ExcelSourceDisplayDriver> localizer)
    {
        _shellOptions = shellOptions;
        _shellSettings = shellSettings;
        S = localizer;
    }

    protected override void EditActivity(ExcelSource activity, ExcelSourceViewModel model)
    {
        model.FilePath = activity.FilePath;
        model.FilesBasePath = GetFilesBasePath();
        model.WorksheetName = activity.WorksheetName;
        model.HasHeaderRow = activity.HasHeaderRow;
    }

    public override async Task<IDisplayResult> UpdateAsync(ExcelSource activity, UpdateEditorContext context)
    {
        var model = new ExcelSourceViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);
        await UpdateActivityAsync(model, activity, context);

        return Edit(activity, context);
    }

    private async Task UpdateActivityAsync(ExcelSourceViewModel model, ExcelSource activity, UpdateEditorContext context)
    {
        if (model.UploadedFile?.Length > 0)
        {
            try
            {
                activity.FilePath = await EtlLocalFilePathResolver.SaveUploadedExcelFileAsync(model.UploadedFile, GetFilesBasePath());
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                activity.FilePath = model.FilePath;
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.UploadedFile), S[ex.Message]);
            }
        }
        else if (!EtlLocalFilePathResolver.IsValidRelativeFilePath(model.FilePath))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.FilePath), S["The file path is invalid."]);
        }
        else if (!EtlLocalFilePathResolver.IsSupportedExcelFilePath(model.FilePath))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.FilePath), S["Only .xlsx files are supported."]);
        }
        else
        {
            activity.FilePath = EtlLocalFilePathResolver.NormalizeRelativeFilePath(model.FilePath).Replace(Path.DirectorySeparatorChar, '/');
        }

        activity.WorksheetName = model.WorksheetName;
        activity.HasHeaderRow = model.HasHeaderRow;
    }

    private string GetFilesBasePath()
    {
        return EtlLocalFilePathResolver.GetFilesBasePath(_shellOptions.Value, _shellSettings);
    }
}
