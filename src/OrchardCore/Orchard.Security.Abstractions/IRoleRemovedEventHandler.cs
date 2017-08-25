using System.Threading.Tasks;
using Orchard.Events;

namespace Orchard.Security
{
    public interface IRoleRemovedEventHandler : IEventHandler
    {
        Task RoleRemovedAsync(string roleName);
    }
}