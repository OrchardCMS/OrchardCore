using System.Collections.Generic;

namespace OrchardCore.Users.ViewModels
{
    public class UsersIndexViewModel
    {
        public IList<UserEntry> Users { get; set; }
        public UserIndexOptions Options { get; set; } = new UserIndexOptions();
        public dynamic Pager { get; set; }
        public dynamic Header { get; set; }
    }

    public class UserEntry
    {
        public dynamic Shape { get; set; }
        public string UserId { get; set; }
    }
}
