namespace OrchardCore.Tenants;

/// <summary>
/// The admin list of feature profiles rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="FeatureProfilesAdminListColumnProvider"/>.
/// </summary>
public static class FeatureProfilesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__FeatureProfiles</c> and <c>AdminListCell__FeatureProfiles__{Column}</c> alternates.
    /// </summary>
    public const string Name = "FeatureProfiles";
}
