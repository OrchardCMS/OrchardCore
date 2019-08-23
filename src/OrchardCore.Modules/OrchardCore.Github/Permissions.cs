using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Github
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageGithubAuthentication
            = new Permission(nameof(ManageGithubAuthentication), "Manage Github Authentication settings");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageGithubAuthentication }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageGithubAuthentication
                }
            };
        }
    }
}
