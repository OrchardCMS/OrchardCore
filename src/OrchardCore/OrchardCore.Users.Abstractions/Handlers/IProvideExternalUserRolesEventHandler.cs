using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Called when a tenant is set up.
    /// </summary>
    public interface IProvideExternalUserRolesEventHandler
    {
        Task UpdateRoles(UpdateRolesContext context);
    }
}
