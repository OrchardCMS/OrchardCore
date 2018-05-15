using System.Collections.Generic;
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
        public IList<string> RoleNames { get; set; } = new List<string>();
        public IList<UserLoginInfo> LoginInfos { get; set; } = new List<UserLoginInfo>();

        public override string ToString()
        {            
            return UserName;
        }
    }
}
