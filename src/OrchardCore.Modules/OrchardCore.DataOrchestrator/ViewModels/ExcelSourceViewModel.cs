using Microsoft.AspNetCore.Http;

namespace OrchardCore.DataOrchestrator.ViewModels;

public class ExcelSourceViewModel
{
    public string FilePath { get; set; }

    public IFormFile UploadedFile { get; set; }

    public string FilesBasePath { get; set; }

    public string WorksheetName { get; set; }

    public bool HasHeaderRow { get; set; } = true;
}
