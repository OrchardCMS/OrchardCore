using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.DataOrchestrator.ViewModels;

public class ContentItemSourceViewModel
{
    public string ContentType { get; set; }

    public string VersionScope { get; set; }

    public string Owner { get; set; }

    public DateTime? CreatedUtcFrom { get; set; }

    public DateTime? CreatedUtcTo { get; set; }

    public IList<SelectListItem> AvailableContentTypes { get; set; } = [];
}
