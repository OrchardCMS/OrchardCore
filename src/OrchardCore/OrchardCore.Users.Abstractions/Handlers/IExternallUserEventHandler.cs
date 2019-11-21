using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Called when a tenant is set up.
    /// </summary>
    public interface IExternallUserEventHandler
    {
        Task ConfigureRoles(ExternalUserContext context);
        Task RequestUsername(ExternalUserContext context);
        Task LoggedIn(ExternalUserContext context);
    }
}
