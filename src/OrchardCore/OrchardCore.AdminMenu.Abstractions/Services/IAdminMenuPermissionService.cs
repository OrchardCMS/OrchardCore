using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.Services
{
    public interface IAdminMenuPermissionService
    {
        Task<IEnumerable<Permission>> GetPermissionsAsync();
    }
}
