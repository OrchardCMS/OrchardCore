using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Twitter
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageTwitter
            = new(nameof(ManageTwitter), "Manage Twitter settings");

        public static readonly Permission ManageTwitterSignin
            = new(nameof(ManageTwitterSignin), "Manage Sign in with Twitter settings");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageTwitter,
                ManageTwitterSignin,
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
                    ManageTwitter,
                    ManageTwitterSignin,
                }
            };
        }
    }
}
