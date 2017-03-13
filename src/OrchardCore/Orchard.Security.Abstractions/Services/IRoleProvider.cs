using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Security.Services
{
    public interface IRoleProvider
    {
        Task<IEnumerable<RoleNamesEntry>> GetRoleNamesAsync();
    }

    public class RoleNamesEntry
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }

    }
}
