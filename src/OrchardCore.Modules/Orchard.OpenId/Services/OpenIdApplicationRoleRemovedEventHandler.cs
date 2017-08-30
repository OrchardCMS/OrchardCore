using System.Threading.Tasks;
using Orchard.Security;

namespace Orchard.OpenId.Services
{
    public class OpenIdApplicationRoleRemovedEventHandler : IRoleRemovedEventHandler
    {
        private readonly OpenIdApplicationStore _store;

        public OpenIdApplicationRoleRemovedEventHandler(OpenIdApplicationStore store)
        {
            _store = store;
        }

        public async Task RoleRemovedAsync(string roleName)
        {
            var apps = await _store.GetAppsInRoleAsync(roleName);

            foreach (var app in apps)
            {
                await _store.RemoveFromRoleAsync(app, roleName);
            }
        }
    }
}