using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    Name = "Administrator",
                    Permissions = new[]
                    {
                        AuditTrailPermissions.ViewAuditTrail,
                        AuditTrailPermissions.ManageAuditTrailSettings
                    }
                },
            };

        public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
            Task.FromResult(new[]
            {
                AuditTrailPermissions.ViewAuditTrail,
                AuditTrailPermissions.ManageAuditTrailSettings
            }
            .AsEnumerable());
    }
}
