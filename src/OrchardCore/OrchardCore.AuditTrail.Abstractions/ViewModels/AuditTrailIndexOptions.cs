using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using YesSql.Filters.Query;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailIndexOptions
    {
        public string SearchText { get; set; }
        public string OriginalSearchText { get; set; }
        public UsersOrder Order { get; set; }
        public UsersFilter Filter { get; set; }
        public string SelectedRole { get; set; }
        public UsersBulkAction BulkAction { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int EventsCount { get; set; }
        public int TotalItemCount { get; set; }

        [ModelBinder(BinderType = typeof(AuditTrailFilterEngineModelBinder), Name = nameof(SearchText))]
        public QueryFilterResult<AuditTrailEvent> FilterResult { get; set; }

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
