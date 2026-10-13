namespace OrchardCore.RateLimits;

/// <summary>
/// The admin list of rate limit policies rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="RateLimitsAdminListColumnDriver"/>.
/// </summary>
public static class RateLimitsAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__RateLimits</c> and <c>AdminListCell__RateLimits__{Column}</c> alternates.
    /// </summary>
    public const string Name = "RateLimits";
}
