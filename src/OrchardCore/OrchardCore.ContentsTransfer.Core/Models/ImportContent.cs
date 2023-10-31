
using Microsoft.AspNetCore.Http;

namespace OrchardCore.ContentsTransfer.Models;

public class ImportContent
{
    public string ContentTypeName { get; set; }

    public string ContentTypeId { get; set; }

    public IFormFile File { get; set; }
}
