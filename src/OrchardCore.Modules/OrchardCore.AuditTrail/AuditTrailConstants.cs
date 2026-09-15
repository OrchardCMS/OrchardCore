namespace OrchardCore.AuditTrail;

/// <summary>
/// The names of the breadcrumbs rendered by the audit trail screens. A module adds a node to one of these trails by
/// registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class AuditTrailConstants
{
    /// <summary>
    /// The breadcrumb of the audit trail list. It is named after the list of that screen.
    /// </summary>
    public const string List = "AuditTrail";

    /// <summary>
    /// The breadcrumb of the audit trail event screen.
    /// </summary>
    public const string Display = "AuditTrailDisplay";
}
