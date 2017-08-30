using System.Threading.Tasks;

namespace Orchard.Security
{
    public interface IRoleRemovedEventHandler
    {
        Task RoleRemovedAsync(string roleName);
    }
}