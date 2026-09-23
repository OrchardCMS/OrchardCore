using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenId;

public static class OpenIdPermissions
{
    public static readonly Permission ManageApplications
        = new("ManageApplications", LocalizedString.Create("View, add, edit and remove the OpenID Connect applications.", typeof(OpenIdPermissions)));

    public static readonly Permission ManageScopes
        = new("ManageScopes", LocalizedString.Create("View, add, edit and remove the OpenID Connect scopes.", typeof(OpenIdPermissions)));

    public static readonly Permission ManageClientSettings
        = new("ManageClientSettings", LocalizedString.Create("View and edit the OpenID Connect client settings.", typeof(OpenIdPermissions)));

    public static readonly Permission ManageServerSettings
        = new("ManageServerSettings", LocalizedString.Create("View and edit the OpenID Connect server settings.", typeof(OpenIdPermissions)));

    public static readonly Permission ManageValidationSettings
        = new("ManageValidationSettings", LocalizedString.Create("View and edit the OpenID Connect validation settings.", typeof(OpenIdPermissions)));
}
