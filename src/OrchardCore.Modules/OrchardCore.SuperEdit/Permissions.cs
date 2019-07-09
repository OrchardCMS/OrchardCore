using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.SuperEdit
{
    public class Permissions : IPermissionProvider
    {
        // Note - in code you should demand PublishContent, EditContent, or DeleteContent
        // Do not demand the "Own" variations - those are applied automatically when you demand the main ones

        public static readonly Permission TenantsManager = new Permission("TenantsManager", "创建网站");
        public static readonly Permission createapp = new Permission("createapp", "创建APP");
        public static readonly Permission thirdcontent = new Permission("thirdcontent", "管理其他平台内容");
        public static readonly Permission webstore = new Permission("webstore", "Manager 管理网上商城");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                TenantsManager,
                createapp,
                thirdcontent,
                webstore
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {
                    TenantsManager,
                    createapp,
                    thirdcontent,
                    webstore
                    }
                }
            };
        }

    }
}