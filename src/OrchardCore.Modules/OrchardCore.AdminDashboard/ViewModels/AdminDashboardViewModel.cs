using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.AdminDashboard.ViewModels
{
    public class AdminDashboardViewModel
    {
        [BindNever]
        public IEnumerable<ContentItem> Widgets { get; set; }
    }
}
