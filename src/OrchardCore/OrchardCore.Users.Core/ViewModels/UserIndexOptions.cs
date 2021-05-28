using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using YesSql.Filters.Query;

namespace OrchardCore.Users.ViewModels
{
    public class UserIndexOptions
    {
        public string SearchText { get; set; }
        public string OriginalSearchText { get; set; }
        public UsersOrder Order { get; set; }
        public UsersFilter Filter { get; set; }
        public string SelectedRole { get; set; }
        public UsersBulkAction BulkAction { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int UsersCount { get; set; }
        public int TotalItemCount { get; set; }

        [ModelBinder(BinderType = typeof(UserFilterEngineModelBinder), Name = nameof(SearchText))]
        public QueryFilterResult<User> FilterResult { get; set; }

        [BindNever]
        public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();

        [BindNever]
        public List<SelectListItem> UserFilters { get; set; }

        [BindNever]
        public List<SelectListItem> UserRoleFilters { get; set; }

        [BindNever]
        public List<SelectListItem> UserSorts { get; set; }

        [BindNever]
        public List<SelectListItem> UsersBulkAction { get; set; }
    }

    public enum UsersOrder
    {
        Name,
        Email,
    }

    public enum UsersFilter
    {
        All,
        Approved,
        Pending,
        EmailPending,
        Enabled,
        Disabled
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
