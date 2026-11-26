namespace OrchardCore.Admin;

/// <summary>
/// The names of the cookies used by the admin, bound from the <c>OrchardCore:Cookies:Admin</c> section, or from the
/// <c>OrchardCore:{TenantName}:Cookies:Admin</c> section for a single tenant.
/// </summary>
/// <remarks>
/// A name that is not configured, or is empty, gets its default value.
/// </remarks>
public sealed class AdminCookieOptions
{
    /// <summary>
    /// The default name of the cookie holding the layouts a user picked for the admin lists.
    /// </summary>
    public const string DefaultListLayout = "orchardcore_admin_list_layout";

    /// <summary>
    /// Gets or sets the name of the cookie holding the layouts a user picked for the admin lists.
    /// Defaults to <c>orchardcore_admin_list_layout</c>.
    /// </summary>
    public string ListLayout { get; set; }
}