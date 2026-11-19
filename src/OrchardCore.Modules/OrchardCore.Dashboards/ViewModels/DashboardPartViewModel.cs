using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Dashboards.Models;

namespace OrchardCore.Dashboards.ViewModels;

public class DashboardPartViewModel
{
    public string ContentItemId { get; set; }

    public double Position { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public string DashboardType { get; set; }

    [BindNever]
    public LocalizedString DashboardTypeDisplayName { get; set; }

    [BindNever]
    public bool IsNew { get; set; }

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public DashboardPart DashboardPart { get; set; }
}
