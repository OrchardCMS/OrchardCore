using System.Collections.Generic;

namespace OrchardCore.Users.Models
{
    public class UsersDocument
    {
        public int Id { get; set; }

        public List<User> Roles { get; } = new List<User>();

        public int Serial { get; set; }
    }
}
