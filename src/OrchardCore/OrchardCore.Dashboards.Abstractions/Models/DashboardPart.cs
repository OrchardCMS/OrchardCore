using OrchardCore.ContentManagement;

namespace OrchardCore.Dashboards.Models;

/// <summary>
/// Stores which dashboard a widget belongs to, and its place on it.
/// </summary>
public class DashboardPart : ContentPart
{
    /// <summary>
    /// Gets or sets the name of the <see cref="DashboardTypeDescriptor"/> the widget belongs to.
    /// </summary>
    public string DashboardType { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user the widget belongs to, on a <see cref="DashboardScope.User"/> dashboard.
    /// </summary>
    public string OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the order of the widget on the dashboard.
    /// </summary>
    public double Position { get; set; }

    /// <summary>
    /// Gets or sets the number of grid columns the widget spans.
    /// </summary>
    public double Width { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the number of grid rows the widget spans.
    /// </summary>
    public double Height { get; set; } = 1.0;
}
