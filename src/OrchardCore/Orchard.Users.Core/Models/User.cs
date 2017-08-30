using System.Collections.Generic;

namespace Orchard.Users.Models
{
    public class User : IUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool EmailConfirmed { get; set; }
        public IList<string> RoleNames { get; set; } = new List<string>();

        public override string ToString()
        {
            return UserName;
        }
    }
}
