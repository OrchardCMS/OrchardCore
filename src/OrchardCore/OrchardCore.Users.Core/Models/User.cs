using System.Collections.Immutable;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Entities;

namespace OrchardCore.Users.Models
{
    public class User : Entity, IUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool EmailConfirmed { get; set; }
        public string ResetToken { get; set; }
        public ImmutableArray<string> RoleNames { get; set; } = ImmutableArray<string>.Empty;
        public ImmutableArray<UserClaim> UserClaims { get; set; } = ImmutableArray<UserClaim>.Empty;
        public ImmutableArray<UserLoginInfo> LoginInfos { get; set; } = ImmutableArray<UserLoginInfo>.Empty;

        public override string ToString()
        {
            return UserName;
        }
    }
}
