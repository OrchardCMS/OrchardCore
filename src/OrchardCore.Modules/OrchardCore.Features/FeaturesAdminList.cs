namespace OrchardCore.Features;

/// <summary>
/// The admin list of features rendered by the <c>AdminList</c> shape. The page renders one list per
/// category, all sharing this name so a column provider configures them together.
/// Its columns are declared by <see cref="FeaturesAdminListColumnProvider"/>.
/// </summary>
public static class FeaturesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Features</c> and <c>AdminListCell__Features__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Features";
}
