using OrchardCore.ContentManagement;

namespace OrchardCore.AdminDashboard.Models
{
    public class DashboardPart : ContentPart
    {
        public double Position { get; set; }
        public double Width { get; set; } = 1.0;
        public double Height { get; set; } = 1.0;
    }
}
