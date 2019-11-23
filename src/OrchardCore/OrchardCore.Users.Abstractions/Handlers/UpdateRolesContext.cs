using System.Linq;
using System.Collections.Generic;

namespace OrchardCore.Users.Handlers
{
    public class UpdateRolesContext : UserContextBase
    {
        public UpdateRolesContext(IUser user, string loginProvider, IEnumerable<SerializableClaim> externalClaims, IEnumerable<string> userRoles) : base(user)
        {
            ExternalClaims = externalClaims.AsEnumerable();
            UserRoles = userRoles;
        }

        public string LoginProvider { get; }

        public IEnumerable<SerializableClaim> ExternalClaims { get; }

        public IEnumerable<string> UserRoles { get; }

        public string[] RolesToAdd { get; } = new string[0];

        public string[] RolesToRemove { get; } = new string[0];
    }
}