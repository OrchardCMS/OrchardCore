namespace OrchardCore.Dashboards;

/// <summary>
/// The request feature set while a dashboard is rendered, to let the widget shapes know which dashboard they are on.
/// </summary>
public class DashboardFeature
{
    /// <summary>
    /// Gets or sets the dashboard type being rendered.
    /// </summary>
    public DashboardTypeDescriptor DashboardType { get; set; }

    /// <summary>
    /// Gets or sets whether the dashboard is rendered in its manage mode, where widgets are arranged.
    /// </summary>
    public bool IsManageRequest { get; set; }
}
