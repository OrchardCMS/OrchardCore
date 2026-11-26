namespace OrchardCore.DisplayManagement;

/// <summary>
/// The names of the cookies used by the display management, bound from the <c>OrchardCore:Cookies:DisplayManagement</c>
/// section, or from the <c>OrchardCore:{TenantName}:Cookies:DisplayManagement</c> section for a single tenant.
/// </summary>
/// <remarks>
/// A name that is not configured, or is empty, gets its default value.
/// </remarks>
public sealed class DisplayManagementCookieOptions
{
    /// <summary>
    /// The default name of the cookie carrying the notifications to the next request.
    /// </summary>
    public const string DefaultNotify = "orch_notify";

    /// <summary>
    /// Gets or sets the name of the cookie carrying the notifications to the next request. Defaults to <c>orch_notify</c>.
    /// </summary>
    public string Notify { get; set; }
}
