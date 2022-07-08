using System;
using System.Threading.Tasks;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.Security;

namespace OrchardCore.OpenId.Services.Handlers
{
    public class OpenIdApplicationRoleRemovedEventHandler : IRoleRemovedEventHandler
    {
        private readonly IOpenIdApplicationManager _manager;

        public OpenIdApplicationRoleRemovedEventHandler(IOpenIdApplicationManager manager)
            => _manager = manager;

        public async Task RoleRemovedAsync(string roleName)
        {
            await foreach (var application in _manager.ListInRoleAsync(roleName))
            {
                var roles = await _manager.GetRolesAsync(application);
                await _manager.SetRolesAsync(application, roles.Remove(roleName, StringComparer.OrdinalIgnoreCase));
            }
        }
    }
}
