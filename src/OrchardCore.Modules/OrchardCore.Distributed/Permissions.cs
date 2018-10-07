using System.Collections.Generic;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Distributed
{
    [Feature("OrchardCore.Distributed.Redis")]
    public class RedisPermissions : IPermissionProvider
    {
        public static readonly Permission ManageRedisServices =
            new Permission("ManageRedisServices", "Manage Redis Services");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageRedisServices };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageRedisServices }
                }
            };
        }
    }
}