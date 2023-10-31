using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.ContentsTransfer.ViewModels;

public class ContentImportViewModel
{
    [Required]
    public string ContentTypeId { get; set; }

    [Required]
    [DataType(DataType.Upload)]
    public IFormFile File { get; set; }
}
