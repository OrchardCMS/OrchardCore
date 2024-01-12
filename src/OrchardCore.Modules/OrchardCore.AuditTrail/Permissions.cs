using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Infrastructure.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AuditTrail
{
    public class Permissions : IPermissionProvider
    {
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
            new[]
            {
                new PermissionStereotype
                {
                    Name = RoleNames.Administrator,
                    Permissions = new[]
                    {
                        AuditTrailPermissions.ViewAuditTrail,
                        AuditTrailPermissions.ManageAuditTrailSettings,
                    }
                },
            };

        public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
            Task.FromResult(new[]
            {
                AuditTrailPermissions.ViewAuditTrail,
                AuditTrailPermissions.ManageAuditTrailSettings,
            }
            .AsEnumerable());
    }
}
