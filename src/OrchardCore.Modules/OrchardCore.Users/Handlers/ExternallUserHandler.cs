using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    public class ExternallUserHandler : IExternallUserEventHandler
    {
        public Task ConfigureRoles(ExternalUserContext context)
        {
            throw new NotImplementedException();
        }

        public Task LoggedIn(ExternalUserContext context)
        {
            throw new NotImplementedException();
        }

        public Task RequestUsername(ExternalUserContext context)
        {
            throw new NotImplementedException();
        }
    }
}
