using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.ViewModels;

public class ListPartFilterViewModel
{
    public string ContentType { get; set; }
    public List<SelectListItem> ContentTypeOptions { get; set; }
    public string DisplayText { get; set; }
    public ContentsStatus Status { get; set; } = ContentsStatus.Latest;
}
