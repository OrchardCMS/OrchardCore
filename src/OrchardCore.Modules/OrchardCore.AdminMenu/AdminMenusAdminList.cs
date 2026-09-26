namespace OrchardCore.AdminMenu;

/// <summary>
/// The admin list of admin menus rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="AdminMenusAdminListColumnProvider"/>.
/// </summary>
public static class AdminMenusAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__AdminMenus</c> and <c>AdminListCell__AdminMenus__{Column}</c> alternates.
    /// </summary>
    public const string Name = "AdminMenus";
}
