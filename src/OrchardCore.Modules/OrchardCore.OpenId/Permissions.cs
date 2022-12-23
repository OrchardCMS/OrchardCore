using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            = new Permission(nameof(ManageValidationSettings), "View and edit the OpenID Connect validation settings.");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageApplications,
                ManageScopes,
                ManageClientSettings,
                ManageServerSettings,
                ManageValidationSettings
            }
            .AsEnumerable());
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
