using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Dashboards.ViewModels;

public class DashboardViewModel
{
    [BindNever]
    public DashboardTypeDescriptor DashboardType { get; set; }

    [BindNever]
    public string Title { get; set; }

    [BindNever]
    public bool CanViewDashboard { get; set; }

    [BindNever]
    public bool CanManageDashboard { get; set; }

    [BindNever]
    public DashboardWrapper[] Dashboards { get; set; }

    [BindNever]
    public List<SelectListItem> Creatable { get; set; }
}
