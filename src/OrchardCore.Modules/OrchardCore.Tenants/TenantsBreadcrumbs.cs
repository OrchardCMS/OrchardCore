namespace OrchardCore.Tenants;

/// <summary>
/// The names of the breadcrumbs rendered by the tenants and feature profiles screens. A module adds a node to one of
/// these trails by registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class TenantsBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the tenants list. It is named after the list of that screen.
    /// </summary>
    public const string List = "Tenants";

    /// <summary>
    /// The breadcrumb of the tenant creation screen.
    /// </summary>
    public const string Create = "TenantsCreate";

    /// <summary>
    /// The breadcrumb of the tenant edition screen.
    /// </summary>
    public const string Edit = "TenantsEdit";

    /// <summary>
    /// The breadcrumb of the feature profiles list. It is named after the list of that screen.
    /// </summary>
    public const string FeatureProfiles = "FeatureProfiles";

    /// <summary>
    /// The breadcrumb of the feature profile creation screen.
    /// </summary>
    public const string FeatureProfilesCreate = "FeatureProfilesCreate";

    /// <summary>
    /// The breadcrumb of the feature profile edition screen.
    /// </summary>
    public const string FeatureProfilesEdit = "FeatureProfilesEdit";
}
