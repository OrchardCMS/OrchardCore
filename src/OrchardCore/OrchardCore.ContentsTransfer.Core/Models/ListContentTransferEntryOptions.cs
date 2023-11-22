using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentsTransfer.Services;
using YesSql.Filters.Query;

namespace OrchardCore.ContentsTransfer.Models;

public class ListContentTransferEntryOptions
{
    public string OriginalSearchText { get; set; }

    public string SearchText { get; set; }

    public ContentTransferEntryStatus? Status { get; set; }

    public ContentTransferEntryOrder? OrderBy { get; set; }

    public ContentTransferEntryBulkAction? BulkAction { get; set; }

    public int EndIndex { get; set; }

    [BindNever]
    public int StartIndex { get; set; }

    [BindNever]
    public int EntriesCount { get; set; }

    [BindNever]
    public int TotalItemCount { get; set; }

    [ModelBinder(BinderType = typeof(ContentTransferEntryFilterEngineModelBinder), Name = nameof(SearchText))]
    public QueryFilterResult<ContentTransferEntry> FilterResult { get; set; }

    [BindNever]
    public List<SelectListItem> BulkActions { get; set; }

    [BindNever]
    public List<SelectListItem> Statuses { get; set; }

    [BindNever]
    public List<SelectListItem> Sorts { get; set; }

    [BindNever]
    public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();

    [BindNever]
    public IList<SelectListItem> ImportableTypes { get; set; }
}
