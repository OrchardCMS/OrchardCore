using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundTasks
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageBackgroundTasks = new("ManageBackgroundTasks", "Manage background tasks");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult<IEnumerable<Permission>>([ManageBackgroundTasks]);
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = [ManageBackgroundTasks],
                }
            };
        }
    }
}
