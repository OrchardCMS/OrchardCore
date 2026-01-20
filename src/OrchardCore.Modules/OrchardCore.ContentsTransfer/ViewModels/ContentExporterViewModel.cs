using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentsTransfer.ViewModels;

public class ContentExporterViewModel
{
    [Required]
    public string Extension { get; set; }

    [Required]
    public string ContentTypeId { get; set; }

    [BindNever]
    public IList<SelectListItem> ContentTypes { get; set; }

    [BindNever]
    public IList<SelectListItem> Extensions { get; set; }

    [BindNever]
    public IShape Content { get; set; }
}
