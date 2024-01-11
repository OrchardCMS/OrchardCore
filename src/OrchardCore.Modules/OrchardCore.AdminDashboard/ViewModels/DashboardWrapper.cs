using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AdminDashboard.ViewModels
{
    public class DashboardWrapper : ShapeViewModel
    {
        public DashboardWrapper() : base("Dashboard_Wrapper")
        {
        }

        public ContentItem Dashboard { get; set; }
        public IShape Content { get; set; }
    }

}
