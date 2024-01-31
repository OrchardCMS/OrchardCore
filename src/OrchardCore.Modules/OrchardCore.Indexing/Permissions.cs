using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageIndexes = new("ManageIndexes", "Manage Indexes");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(_permissions);
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = _permissions,
                },
            };
        }

        private readonly IEnumerable<Permission> _permissions =
        [
            ManageIndexes
        ];
    }
}
