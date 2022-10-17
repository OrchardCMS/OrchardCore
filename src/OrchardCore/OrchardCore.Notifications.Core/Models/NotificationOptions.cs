using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using YesSql.Filters.Query;

namespace OrchardCore.Notifications.Models;

public class ListNotificationOptions
{
    public string OriginalSearchText { get; set; }

    public string SearchText { get; set; }

    public NotificationStatus? Status { get; set; }

    public NotificationOrder? Sort { get; set; }

    public int EndIndex { get; set; }

    [BindNever]
    public int StartIndex { get; set; }

    [BindNever]
    public int ContentItemsCount { get; set; }

    [BindNever]
    public int TotalItemCount { get; set; }

    [ModelBinder(BinderType = typeof(WebNotificationFilterEngineModelBinder), Name = nameof(SearchText))]
    public QueryFilterResult<WebNotification> FilterResult { get; set; }

    [BindNever]
    public List<SelectListItem> ContentsBulkAction { get; set; }

    [BindNever]
    public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();
}
