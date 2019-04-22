using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Github
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageGithubAuthentication
            = new Permission(nameof(ManageGithubAuthentication), "Manage Github Authentication settings");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ManageGithubAuthentication;
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
