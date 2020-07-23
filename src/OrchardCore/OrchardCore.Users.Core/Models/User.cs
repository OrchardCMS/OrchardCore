using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Entities;

namespace OrchardCore.Users.Models
{
    public class User : Entity, IUser
    {
        public int Id { get; set; }
        private string _userId;
        public string UserId
        {
            get
            {
                if (String.IsNullOrEmpty(_userId))
                {
                    // Ensure that an update to this user will save the original user name when it has been changed.
                    _userId = NormalizedUserName;
                    return NormalizedUserName;
                }
                else
                {
                    return _userId;
                }
            }
            set => _userId = value;
        }

        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsEnabled { get; set; } = true;
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
