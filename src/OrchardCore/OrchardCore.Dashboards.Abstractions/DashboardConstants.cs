namespace OrchardCore.Dashboards;

public static class DashboardConstants
{
    /// <summary>
    /// The stereotype of the content types that can be added as widgets to a dashboard.
    /// </summary>
    public const string WidgetStereotype = "DashboardWidget";

    /// <summary>
    /// The name of the query string value giving the dashboard type of a widget being created.
    /// </summary>
    public const string DashboardTypeQueryName = "dashboardType";

    /// <summary>
    /// The name of the dashboard type every user has for themselves, registered by the <see cref="Features.User"/> feature.
    /// </summary>
    public const string UserDashboardType = "MyDashboard";

    public static class Features
    {
        public const string Dashboards = "OrchardCore.Dashboards";

        public const string User = "OrchardCore.Dashboards.User";
    }
}
