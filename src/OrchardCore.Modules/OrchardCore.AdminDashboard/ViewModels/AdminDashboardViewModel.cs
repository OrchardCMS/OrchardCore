using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.AdminDashboard.ViewModels
{
    public class AdminDashboardViewModel
    {
        [BindNever]
        public bool CanManageDashboard { get; set; }

        [BindNever]
        public DashboardWrapper[] Dashboards { get; set; }

        [BindNever]
        public List<SelectListItem> Creatable { get; set; }
    }
}
