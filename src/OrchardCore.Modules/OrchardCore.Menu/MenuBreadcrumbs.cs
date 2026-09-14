namespace OrchardCore.Menu;

/// <summary>
/// The names of the breadcrumbs rendered by the menu screens, and the keys of the contextual data each of them
/// carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class MenuBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the menu item creation screen. It carries <see cref="ItemTypeDisplayNameKey"/>, and
    /// <see cref="MenuContentItemIdKey"/> when the item belongs to a menu.
    /// </summary>
    public const string Create = "MenuCreate";

    /// <summary>
    /// The breadcrumb of the menu item edition screen. It carries <see cref="ItemTypeDisplayNameKey"/> and
    /// <see cref="MenuContentItemIdKey"/>.
    /// </summary>
    public const string Edit = "MenuEdit";

    /// <summary>
    /// The key under which a screen passes the display name of the menu item type it is about.
    /// </summary>
    public const string ItemTypeDisplayNameKey = "ItemTypeDisplayName";

    /// <summary>
    /// The key under which a screen passes the identifier of the menu the item belongs to. Empty when the menu itself
    /// is being created.
    /// </summary>
    public const string MenuContentItemIdKey = "MenuContentItemId";
}
