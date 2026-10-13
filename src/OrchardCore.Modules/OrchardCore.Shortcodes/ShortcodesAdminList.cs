namespace OrchardCore.Shortcodes;

/// <summary>
/// The admin list of shortcode templates rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="ShortcodesAdminListColumnDriver"/>.
/// </summary>
public static class ShortcodesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__Shortcodes</c> and <c>AdminListCell__Shortcodes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Shortcodes";
}
