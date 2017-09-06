using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Demo 
{
    public class Permissions : IPermissionProvider 
    {
        // Note - in code you should demand PublishContent, EditContent, or DeleteContent
        // Do not demand the "Own" variations - those are applied automatically when you demand the main ones

        public static readonly Permission DemoAPIAccess = new Permission("DemoAPIAccess", "Access to Demo API ");
        
        public IEnumerable<Permission> GetPermissions() {
            return new[] {DemoAPIAccess};
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() 
        {
            return new[] 
            {
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] { DemoAPIAccess }
                }
            };
        }

    }
}