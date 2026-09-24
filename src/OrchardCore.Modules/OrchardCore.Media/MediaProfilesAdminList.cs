namespace OrchardCore.Media;

/// <summary>
/// The admin list of media profiles rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="MediaProfilesAdminListColumnProvider"/>.
/// </summary>
public static class MediaProfilesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__MediaProfiles</c> and <c>AdminListCell__MediaProfiles__{Column}</c> alternates.
    /// </summary>
    public const string Name = "MediaProfiles";
}
