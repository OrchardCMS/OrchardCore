namespace OrchardCore.Tenants;

/// <summary>
/// The admin list of feature profiles rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="FeatureProfilesAdminListColumnDriver"/>.
/// </summary>
public static class FeatureProfilesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__FeatureProfiles</c> and <c>AdminListCell__FeatureProfiles__{Column}</c> alternates.
    /// </summary>
    public const string Name = "FeatureProfiles";
}
