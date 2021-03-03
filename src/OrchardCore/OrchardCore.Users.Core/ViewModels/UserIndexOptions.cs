using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Users.ViewModels
{
    public class UserIndexOptions
    {
        public string Search { get; set; }
        public UsersOrder Order { get; set; }
        public UsersFilter Filter { get; set; }
        public UsersBulkAction BulkAction { get; set; }

        [BindNever]
        public List<SelectListItem> UserFilters { get; set; }

        [BindNever]
        public List<SelectListItem> UserSorts { get; set; }

        [BindNever]
        public List<SelectListItem> UsersBulkAction { get; set; }
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
        Enable,
        Disable,
        Approve,
        ChallengeEmail
    }
}
