using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentsTransfer.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace CrestApps.Contents.Imports.Drivers;

public class ImportContentDisplayDriver : DisplayDriver<ImportContent>
{
    private readonly ContentImportOptions _contentImportOptions;
    protected readonly IStringLocalizer S;

    private static readonly HashSet<string> _allowedExtensions = new()
    {
        ".csv",
        ".xls",
        ".xlsx"
    };

    public ImportContentDisplayDriver(
        IOptions<ContentImportOptions> contentImportOptions,
        IStringLocalizer<ImportContentDisplayDriver> stringLocalizer
        )
    {
        _contentImportOptions = contentImportOptions.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ImportContent model)
    {
        return Initialize<ContentImportViewModel>("ImportContentFile_Edit", viewModel => viewModel.File = model.File)
            .Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(ImportContent model, UpdateEditorContext context)
    {
        var viewModel = new ContentImportViewModel();

        if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            if (viewModel.File?.Length == 0)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.File), S["File is required."]);
            }
            else
            {
                var extension = Path.GetExtension(viewModel.File.FileName);

                if (!_allowedExtensions.Contains(extension))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.File), S["This extension is not allowed"]);
                }

                if (_contentImportOptions.MaxAllowedFileSizeInBytes > 0 && viewModel.File.Length > _contentImportOptions.MaxAllowedFileSizeInBytes)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.File), S["The uploaded file size exceeded the allowed file size {0} MB", _contentImportOptions.GetMaxAllowedSizeInMb()]);
                }

                model.File = viewModel.File;
            }
        }

        return Edit(model);
    }
}
