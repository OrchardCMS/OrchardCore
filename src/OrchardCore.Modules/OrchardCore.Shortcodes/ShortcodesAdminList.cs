namespace OrchardCore.Shortcodes;

/// <summary>
/// The admin list of shortcode templates rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="ShortcodesAdminListColumnProvider"/>.
/// </summary>
public static class ShortcodesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Shortcodes</c> and <c>AdminListCell__Shortcodes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Shortcodes";
}
