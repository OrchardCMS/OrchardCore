using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security.Services
{
    /// <summary>
    /// Service that returns the installed permissions
    /// </summary>
    public interface IPermissionsService
    {
        Task<IEnumerable<Permission>> GetInstalledPermissionsAsync();
    }
}
