using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentsTransfer.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace CrestApps.Contents.Imports.Drivers;

public class ImportContentDisplayDriver : DisplayDriver<ImportContent>
{
    protected readonly IStringLocalizer S;

    private static readonly HashSet<string> _allowedExtensions = new()
    {
        ".csv",
        ".xls",
        ".xlsx"
    };

    public ImportContentDisplayDriver(
        IStringLocalizer<ImportContentDisplayDriver> stringLocalizer
        )
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ImportContent model)
        => Initialize<ContentImportViewModel>("ImportContentFile_Edit", viewModel => viewModel.File = model.File)
            .Location("Content:1");

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

                model.File = viewModel.File;
            }
        }

        return Edit(model);
    }
}
