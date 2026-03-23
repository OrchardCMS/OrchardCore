using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.ContentTransfer.ViewModels;

public class ContentImportViewModel
{
    [Required]
    [DataType(DataType.Upload)]
    public IFormFile File { get; set; }
}
