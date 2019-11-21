using System.Linq;
using System.Collections.Generic;

namespace OrchardCore.Users.Handlers
{
    public class UpdateRolesContext : UserContextBase
    {
        public UpdateRolesContext(IUser user, IEnumerable<ExternalUserClaim> claims, IEnumerable<string> currentRoles) : base(user)
        {
            Claims = claims.AsEnumerable();
            CurrentRoles = currentRoles;
        }

        public IEnumerable<ExternalUserClaim> Claims { get; }

        public IEnumerable<string> CurrentRoles { get; }

        public string[] RolesToAdd { get; } = new string[0];

        public string[] RolesToRemove { get; } = new string[0];
    }
}