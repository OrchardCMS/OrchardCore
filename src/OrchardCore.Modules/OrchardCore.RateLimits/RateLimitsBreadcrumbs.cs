namespace OrchardCore.RateLimits;

/// <summary>
/// The names of the breadcrumbs rendered by the rate limits screens, and the keys of the contextual data each of them
/// carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class RateLimitsBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the rate limit policies list. It is named after the list of that screen.
    /// </summary>
    public const string List = "RateLimits";

    /// <summary>
    /// The breadcrumb of the rate limit policy creation screen.
    /// </summary>
    public const string Create = "RateLimitsCreate";

    /// <summary>
    /// The breadcrumb of the rate limit policy edition screen.
    /// </summary>
    public const string Edit = "RateLimitsEdit";

    /// <summary>
    /// The breadcrumb of the limiter creation screen. It carries <see cref="DisplayNameKey"/>.
    /// </summary>
    public const string LimiterCreate = "RateLimitsLimiterCreate";

    /// <summary>
    /// The breadcrumb of the limiter edition screen. It carries <see cref="DisplayNameKey"/>.
    /// </summary>
    public const string LimiterEdit = "RateLimitsLimiterEdit";

    /// <summary>
    /// The key under which a limiter screen passes the display name of the limiter it is about.
    /// </summary>
    public const string DisplayNameKey = "DisplayName";

    /// <summary>
    /// The key under which a limiter screen passes the identifier of the policy it belongs to.
    /// </summary>
    public const string PolicyIdKey = "PolicyId";
}
