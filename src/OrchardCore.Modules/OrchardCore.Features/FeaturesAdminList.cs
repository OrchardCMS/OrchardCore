namespace OrchardCore.Features;

/// <summary>
/// The admin list of features rendered by the <c>AdminList</c> shape. The page renders one list per
/// category, all sharing this name so the same column providers declare their columns.
/// Its columns are declared by the rendering view.
/// </summary>
public static class FeaturesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column providers and used by the
    /// <c>AdminList__Features</c> and <c>AdminListCell__Features__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Features";
}
