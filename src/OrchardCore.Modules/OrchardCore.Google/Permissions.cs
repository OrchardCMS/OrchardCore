using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageGoogleAuthentication
            = new Permission(nameof(ManageGoogleAuthentication), "Manage Google Authentication settings");

        public static readonly Permission ManageGoogleAnalytics
            = new Permission(nameof(ManageGoogleAnalytics), "Manage Google Analytics settings");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageGoogleAuthentication,
                ManageGoogleAnalytics
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
                    ManageGoogleAuthentication,
                    ManageGoogleAnalytics
                }
            };
        }
    }
}
