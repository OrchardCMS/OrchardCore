using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenId
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageApplications
            = new(nameof(ManageApplications), "View, add, edit and remove the OpenID Connect applications.");

        public static readonly Permission ManageScopes
            = new(nameof(ManageScopes), "View, add, edit and remove the OpenID Connect scopes.");

        public static readonly Permission ManageClientSettings
            = new(nameof(ManageClientSettings), "View and edit the OpenID Connect client settings.");

        public static readonly Permission ManageServerSettings
            = new(nameof(ManageServerSettings), "View and edit the OpenID Connect server settings.");

        public static readonly Permission ManageValidationSettings
            = new(nameof(ManageValidationSettings), "View and edit the OpenID Connect validation settings.");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageApplications,
                ManageScopes,
                ManageClientSettings,
                ManageServerSettings,
                ManageValidationSettings,
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
                    ManageValidationSettings,
                }
            };
        }
    }
}
