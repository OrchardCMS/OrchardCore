/*
	Permission class
		Gives the 'ManageBackgoundTasks' right for the 'admin' role
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundTasks
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageBackgroundTasks = new Permission("ManageBackgroundTasks", "Manage background tasks");

        // @return 'Permission' object (permission/access/set of rights)
        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(GetPermissions());
        }

        // @return collection of roles including name and set of rights,
        //         in this case, the role of 'Administrator' with the right 'ManageBackgroundTasks'
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] 
            { 
                new PermissionStereotype 
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageBackgroundTasks }
                }
            };
        }

        // @return set of rights set by all objects of this class
        private IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageBackgroundTasks };
        }
    }
}
