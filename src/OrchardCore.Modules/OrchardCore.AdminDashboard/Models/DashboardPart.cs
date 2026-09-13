using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.AdminDashboard.Models;

public class DashboardPart : ContentPart
{
    [Description("The dashboard item's ordering position.")]
    public double Position { get; set; }

    [Description("The dashboard item's width in grid units.")]
    public double Width { get; set; } = 1.0;

    [Description("The dashboard item's height in grid units.")]
    public double Height { get; set; } = 1.0;
}
