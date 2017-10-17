using System.Collections.Generic;

namespace OrchardCore.Users.ViewModels
{
    public class UsersIndexViewModel
    {
        public IList<UserEntry> Users { get; set; }
        public UserIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class UserEntry
    {
        public dynamic Shape { get; set; }
        public bool IsChecked { get; set; }
    }

    public class UserIndexOptions
    {
        public string Search { get; set; }
        public UsersOrder Order { get; set; }
        public UsersFilter Filter { get; set; }
        public UsersBulkAction BulkAction { get; set; }
    }

    public enum UsersOrder
    {
        Name,
        Email,
        CreatedUtc,
        LastLoginUtc
    }

    public enum UsersFilter
    {
        All,
        Approved,
        Pending,
        EmailPending
    }

    public enum UsersBulkAction
    {
        None,
        Delete,
        Disable,
        Approve,
        ChallengeEmail
    }
}
