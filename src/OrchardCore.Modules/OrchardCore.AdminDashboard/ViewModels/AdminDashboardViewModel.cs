using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.AdminDashboard.ViewModels
{
    public class AdminDashboardViewModel
    {
        [BindNever]
        public List<dynamic> Widgets { get; set; } = new List<dynamic>();
    }
}
