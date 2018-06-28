using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenId
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageApplications
            = new Permission(nameof(ManageApplications), "View, add, edit and remove the OpenID Connect applications.");

        public static readonly Permission ManageScopes
            = new Permission(nameof(ManageScopes), "View, add, edit and remove the OpenID Connect scopes.");

        public static readonly Permission ManageClientSettings
            = new Permission(nameof(ManageClientSettings), "View and edit the OpenID Connect client settings.");

        public static readonly Permission ManageServerSettings
            = new Permission(nameof(ManageServerSettings), "View and edit the OpenID Connect server settings.");

        public static readonly Permission ManageValidationSettings
            = new Permission(nameof(ManageValidationSettings), "View and edit the OpenID Connect server settings.");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ManageApplications;
            yield return ManageScopes;
            yield return ManageClientSettings;
            yield return ManageServerSettings;
            yield return ManageValidationSettings;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageApplications,
                    ManageScopes,
                    ManageClientSettings,
                    ManageServerSettings,
                    ManageValidationSettings
                }
            };
        }
    }
}
