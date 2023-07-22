using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.GitHub
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageGitHubAuthentication
            = new(nameof(ManageGitHubAuthentication), "Manage GitHub Authentication settings");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageGitHubAuthentication }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageGitHubAuthentication,
                }
            };
        }
    }
}
