namespace OrchardCore.Roles;

/// <summary>
/// The names of the breadcrumbs rendered by the roles screens, and the keys of the contextual data each of them
/// carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class RolesBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the roles list. It is named after the list of that screen.
    /// </summary>
    public const string List = "Roles";

    /// <summary>
    /// The breadcrumb of the role creation screen.
    /// </summary>
    public const string Create = "RolesCreate";

    /// <summary>
    /// The breadcrumb of the role edition screen. It carries <see cref="RoleNameKey"/>.
    /// </summary>
    public const string Edit = "RolesEdit";

    /// <summary>
    /// The breadcrumb of the role cloning screen. It carries <see cref="RoleNameKey"/>.
    /// </summary>
    public const string Clone = "RolesClone";

    /// <summary>
    /// The breadcrumb of the role permissions screen. It carries <see cref="RoleNameKey"/>.
    /// </summary>
    public const string Display = "RolesDisplay";

    /// <summary>
    /// The key under which the screen passes the name of the role it is about.
    /// </summary>
    public const string RoleNameKey = "RoleName";
}
