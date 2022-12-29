using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Notifications.Services;
using YesSql.Filters.Query;

namespace OrchardCore.Notifications.Models;

public class ListNotificationOptions
{
    public string OriginalSearchText { get; set; }

    public string SearchText { get; set; }

    public NotificationStatus? Status { get; set; }

    public NotificationOrder? OrderBy { get; set; }

    public NotificationBulkAction? BulkAction { get; set; }

    public int EndIndex { get; set; }

    [BindNever]
    public int StartIndex { get; set; }

    [BindNever]
    public int NotficationsItemsCount { get; set; }

    [BindNever]
    public int TotalItemCount { get; set; }

    [ModelBinder(BinderType = typeof(NotificationFilterEngineModelBinder), Name = nameof(SearchText))]
    public QueryFilterResult<Notification> FilterResult { get; set; }

    [BindNever]
    public List<SelectListItem> BulkActions { get; set; }

    [BindNever]
    public List<SelectListItem> Statuses { get; set; }

    [BindNever]
    public List<SelectListItem> Sorts { get; set; }

    [BindNever]
    public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();
}
