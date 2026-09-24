namespace OrchardCore.RateLimits;

/// <summary>
/// The admin list of rate limit policies rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="RateLimitsAdminListColumnProvider"/>.
/// </summary>
public static class RateLimitsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="OrchardCore.Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__RateLimits</c> and <c>AdminListCell__RateLimits__{Column}</c> alternates.
    /// </summary>
    public const string Name = "RateLimits";
}
