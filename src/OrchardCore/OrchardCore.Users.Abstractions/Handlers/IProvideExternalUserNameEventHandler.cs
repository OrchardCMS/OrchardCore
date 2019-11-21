using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Called when a tenant is set up.
    /// </summary>
    public interface IProvideExternalUserNameEventHandler
    {
        Task<string> RequestUserName(string provider, IEnumerable<ExternalUserClaim> claims);
    }
}
