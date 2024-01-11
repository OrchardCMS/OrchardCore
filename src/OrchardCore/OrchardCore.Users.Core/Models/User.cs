using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Entities;

namespace OrchardCore.Users.Models
{
    public class User : Entity, IUser
    {
        public long Id { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string NormalizedEmail { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool IsEnabled { get; set; } = true;

        public bool TwoFactorEnabled { get; set; }

        public bool IsLockoutEnabled { get; set; }

        public DateTime? LockoutEndUtc { get; set; }

        public int AccessFailedCount { get; set; }

        public string ResetToken { get; set; }

        public IList<string> RoleNames { get; set; } = new List<string>();

        public IList<UserClaim> UserClaims { get; set; } = new List<UserClaim>();

        public IList<UserLoginInfo> LoginInfos { get; set; } = new List<UserLoginInfo>();

        public IList<UserToken> UserTokens { get; set; } = new List<UserToken>();

        public override string ToString()
        {
            return UserName;
        }
    }
}
