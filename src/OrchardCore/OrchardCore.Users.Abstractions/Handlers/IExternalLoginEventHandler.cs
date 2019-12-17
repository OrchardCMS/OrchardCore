using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Called when a tenant is set up.
    /// </summary>
    public interface IExternalLoginEventHandler
    {
        Task<string> GenerateUserName(string provider, IEnumerable<SerializableClaim> claims);

        Task UpdateRoles(UpdateRolesContext context);
    }
}
