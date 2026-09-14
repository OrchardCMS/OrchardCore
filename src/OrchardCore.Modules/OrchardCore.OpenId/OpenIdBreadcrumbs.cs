namespace OrchardCore.OpenId;

/// <summary>
/// The names of the breadcrumbs rendered by the OpenID screens. A module adds a node to one of these trails by
/// registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class OpenIdBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the applications list. It is named after the list of that screen.
    /// </summary>
    public const string ApplicationsList = "OpenIdApplications";

    /// <summary>
    /// The breadcrumb of the application creation screen.
    /// </summary>
    public const string ApplicationsCreate = "OpenIdApplicationsCreate";

    /// <summary>
    /// The breadcrumb of the application edition screen.
    /// </summary>
    public const string ApplicationsEdit = "OpenIdApplicationsEdit";

    /// <summary>
    /// The breadcrumb of the scopes list. It is named after the list of that screen.
    /// </summary>
    public const string ScopesList = "OpenIdScopes";

    /// <summary>
    /// The breadcrumb of the scope creation screen.
    /// </summary>
    public const string ScopesCreate = "OpenIdScopesCreate";

    /// <summary>
    /// The breadcrumb of the scope edition screen.
    /// </summary>
    public const string ScopesEdit = "OpenIdScopesEdit";

    /// <summary>
    /// The breadcrumb of the server settings screen.
    /// </summary>
    public const string ServerConfiguration = "OpenIdServerConfiguration";

    /// <summary>
    /// The breadcrumb of the validation settings screen.
    /// </summary>
    public const string ValidationConfiguration = "OpenIdValidationConfiguration";
}
