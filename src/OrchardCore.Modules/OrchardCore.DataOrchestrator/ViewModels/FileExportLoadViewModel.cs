using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.DataOrchestrator.ViewModels;

/// <summary>
/// Base view model for file-based export destinations sharing the format and file-name fields.
/// </summary>
public abstract class FileExportLoadViewModel
{
    public string Format { get; set; }

    public string FileName { get; set; }

    [BindNever]
    public IList<SelectListItem> Formats { get; set; } = [];
}
