namespace OrchardCore.OpenId;

/// <summary>
/// The admin list of OpenID applications rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="OpenIdApplicationsAdminListColumnProvider"/>.
/// </summary>
public static class OpenIdApplicationsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__OpenIdApplications</c> and <c>AdminListCell__OpenIdApplications__{Column}</c> alternates.
    /// </summary>
    public const string Name = "OpenIdApplications";
}

/// <summary>
/// The admin list of OpenID scopes rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="OpenIdScopesAdminListColumnProvider"/>.
/// </summary>
public static class OpenIdScopesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__OpenIdScopes</c> and <c>AdminListCell__OpenIdScopes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "OpenIdScopes";
}
