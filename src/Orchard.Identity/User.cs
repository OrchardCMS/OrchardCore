using System.Collections.Generic;

namespace Orchard.Identity
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string PasswordHash { get; set; }
        public List<string> RoleNames { get; set; } = new List<string>();
    }
}
