using System.Threading.Tasks;
using Orchard.Roles.Models;
using YesSql.Core.Services;

namespace Orchard.Roles.Services
{
    public class RoleManager : IRoleManager
    {
        private readonly ISession _session;
        private RolesDocument _roles;

        public RoleManager(ISession session)
        {
            _session = session;
        }

        public async Task<RolesDocument> GetRolesAsync()
        {
            if(_roles == null)
            {
                _roles = await _session.QueryAsync<RolesDocument>().FirstOrDefault();

                if(_roles == null)
                {
                    _roles = new RolesDocument();
                    UpdateRoles();
                }
            }

            return _roles;
        }

        public void UpdateRoles()
        {
            if (_roles != null)
            {
                _session.Save(_roles);
            }
        }
    }
}
