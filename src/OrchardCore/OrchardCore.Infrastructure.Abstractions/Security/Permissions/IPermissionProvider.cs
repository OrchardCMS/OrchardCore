using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Security.Permissions
{
    /// <summary>
    /// Implemented by modules to enumerate the types of permissions
    /// the which may be granted
    /// </summary>
    public interface IPermissionProvider
    {
        IEnumerable<Permission> GetPermissions();
        IEnumerable<PermissionStereotype> GetDefaultStereotypes();
    }

    /// <summary>
    /// Async version of <see cref="IPermissionProvider"/>.
    /// </summary>
    public interface IAsyncPermissionProvider : IPermissionProvider
    {
        Task<IEnumerable<Permission>> GetPermissionsAsync();
    }

    public class PermissionStereotype
    {
        public string Name { get; set; }
        public IEnumerable<Permission> Permissions { get; set; }
    }
}
