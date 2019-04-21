using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Github
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageGithubSignin
            = new Permission(nameof(ManageGithubSignin), "Manage Sign in with Github settings");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ManageGithubSignin;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageGithubSignin
                }
            };
        }
    }
}
