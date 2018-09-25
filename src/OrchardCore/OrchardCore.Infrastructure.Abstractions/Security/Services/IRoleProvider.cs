using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Security.Services
{
    public interface IRoleProvider
    {
        Task<IEnumerable<string>> GetRoleNamesAsync();
    }

}
