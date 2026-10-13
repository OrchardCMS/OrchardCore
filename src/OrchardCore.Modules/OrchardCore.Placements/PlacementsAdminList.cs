namespace OrchardCore.Placements;

/// <summary>
/// The admin list of shape placements rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="PlacementsAdminListColumnDriver"/>.
/// </summary>
public static class PlacementsAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__Placements</c> and <c>AdminListCell__Placements__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Placements";
}
