namespace OrchardCore.Media;

/// <summary>
/// The admin list of media profiles rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="MediaProfilesAdminListColumnDriver"/>.
/// </summary>
public static class MediaProfilesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__MediaProfiles</c> and <c>AdminListCell__MediaProfiles__{Column}</c> alternates.
    /// </summary>
    public const string Name = "MediaProfiles";
}
