namespace OrchardCore.AdminMenu;

/// <summary>
/// The names of the breadcrumbs rendered by the admin menus screens, and the keys of the contextual data each of them
/// carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class AdminMenuConstants
{
    /// <summary>
    /// The breadcrumb of the admin menus list. It is named after the list of that screen.
    /// </summary>
    public const string List = "AdminMenus";

    /// <summary>
    /// The breadcrumb of the admin menu creation screen.
    /// </summary>
    public const string Create = "AdminMenusCreate";

    /// <summary>
    /// The breadcrumb of the admin menu edition screen. It carries <see cref="MenuNameKey"/>.
    /// </summary>
    public const string Edit = "AdminMenusEdit";

    /// <summary>
    /// The breadcrumb of the admin menu nodes screen. It carries <see cref="MenuNameKey"/>.
    /// </summary>
    public const string Nodes = "AdminMenusNodes";

    /// <summary>
    /// The breadcrumb of the node creation screen.
    /// </summary>
    public const string NodeCreate = "AdminMenusNodeCreate";

    /// <summary>
    /// The breadcrumb of the node edition screen.
    /// </summary>
    public const string NodeEdit = "AdminMenusNodeEdit";

    /// <summary>
    /// The key under which a screen passes the name of the admin menu it is about.
    /// </summary>
    public const string MenuNameKey = "MenuName";

    /// <summary>
    /// The key under which a node screen passes the identifier of the admin menu it belongs to.
    /// </summary>
    public const string MenuIdKey = "MenuId";
}
