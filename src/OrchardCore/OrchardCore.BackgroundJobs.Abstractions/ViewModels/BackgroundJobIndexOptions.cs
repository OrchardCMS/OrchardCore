using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundJobs.Services;
using YesSql.Filters.Query;

namespace OrchardCore.BackgroundJobs.ViewModels
{
    public class BackgroundJobIndexOptions
    {
        public string SearchText { get; set; }
        public string OriginalSearchText { get; set; }
        public string BackgroundJobName { get; set; }
        public string CorrelationId { get; set; }
        public string RepeatCorrelationId { get; set; }
        public BackgroundJobOrder Order { get; set; }
        public BackgroundJobStatus Filter { get; set; }
        public BackgroundJobBulkAction BulkAction { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int BackgrounJobsCount { get; set; }
        public int TotalItemCount { get; set; }

        [ModelBinder(BinderType = typeof(BackgroundJobFilterEngineModelBinder), Name = nameof(SearchText))]
        public QueryFilterResult<BackgroundJobExecution> FilterResult { get; set; }

        [BindNever]
        public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();

        [BindNever]
        public List<SelectListItem> UserFilters { get; set; }

        [BindNever]
        public List<SelectListItem> UserSorts { get; set; }

        [BindNever]
        public List<SelectListItem> UsersBulkAction { get; set; }
    }

    public enum BackgroundJobOrder
    {
        Name,
        Email,
    }



    public enum BackgroundJobBulkAction
    {
        None,
        Rerun,
        Delete
    }
}
