using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenId;

public static class OpenIdPermissions
{
    public static readonly Permission ManageApplications
        = new("ManageApplications", new LocalizationSource("View, add, edit and remove the OpenID Connect applications.", typeof(OpenIdPermissions)));

    public static readonly Permission ManageScopes
        = new("ManageScopes", new LocalizationSource("View, add, edit and remove the OpenID Connect scopes.", typeof(OpenIdPermissions)));

    public static readonly Permission ManageClientSettings
        = new("ManageClientSettings", new LocalizationSource("View and edit the OpenID Connect client settings.", typeof(OpenIdPermissions)));

    public static readonly Permission ManageServerSettings
        = new("ManageServerSettings", new LocalizationSource("View and edit the OpenID Connect server settings.", typeof(OpenIdPermissions)));

    public static readonly Permission ManageValidationSettings
        = new("ManageValidationSettings", new LocalizationSource("View and edit the OpenID Connect validation settings.", typeof(OpenIdPermissions)));
}
