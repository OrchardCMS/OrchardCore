using OrchardCore.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.AuditTrail.Permissions
{
    public class AuditTrailPermissions : IPermissionProvider
    {
        public static readonly Permission ViewAuditTrail = new Permission(nameof(ViewAuditTrail), "View audit trail");
        public static readonly Permission ManageAuditTrailSettings = new Permission(nameof(ManageAuditTrailSettings), "Manage audit trail settings");

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
            new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[]
                    {
                        ViewAuditTrail,
                        ManageAuditTrailSettings
                    }
                },
            };

        public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
            Task.FromResult(new[]
            {
                ViewAuditTrail,
                ManageAuditTrailSettings
            }
            .AsEnumerable());
    }
}
