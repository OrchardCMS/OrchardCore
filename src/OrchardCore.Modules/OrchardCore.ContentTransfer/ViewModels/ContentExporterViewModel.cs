using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentTransfer.ViewModels;

public class ContentExporterViewModel
{
    [Required]
    public string Extension { get; set; }

    [Required]
    public string ContentTypeId { get; set; }

    public bool PartialExport { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public DateTime? ModifiedFrom { get; set; }

    public DateTime? ModifiedTo { get; set; }

    public string Owners { get; set; }

    public bool PublishedOnly { get; set; } = true;

    public bool LatestOnly { get; set; }

    public bool AllVersions { get; set; }

    [BindNever]
    public IList<SelectListItem> ContentTypes { get; set; }

    [BindNever]
    public IList<SelectListItem> Extensions { get; set; }

    [BindNever]
    public IShape Content { get; set; }
}
