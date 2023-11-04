using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.ContentsTransfer.ViewModels;

public class ContentImportViewModel
{
    [Required]
    [DataType(DataType.Upload)]
    public IFormFile File { get; set; }
}
