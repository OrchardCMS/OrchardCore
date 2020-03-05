using System.Threading.Tasks;

namespace OrchardCore.Security
{
    public interface IRoleRemovedEventHandler
    {
        Task RoleRemovedAsync(string roleName);
    }
}
