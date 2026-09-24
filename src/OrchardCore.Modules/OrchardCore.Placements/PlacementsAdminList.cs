namespace OrchardCore.Placements;

/// <summary>
/// The admin list of shape placements rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="PlacementsAdminListColumnProvider"/>.
/// </summary>
public static class PlacementsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="OrchardCore.Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Placements</c> and <c>AdminListCell__Placements__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Placements";
}
