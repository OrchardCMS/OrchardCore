using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Orchard.Security.Services
{
    public interface IRoleProvider
    {
        Task<IEnumerable<string>> GetRoleNamesAsync();
    }

}
