using System;
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
        public string Category { get; set; }
        public string UserName { get; set; }
        public string CorrelationId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public AuditTrailSort Sort { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int EventsCount { get; set; }
        public int TotalItemCount { get; set; }

        [ModelBinder(BinderType = typeof(AuditTrailFilterEngineModelBinder), Name = nameof(SearchText))]
        public QueryFilterResult<AuditTrailEvent> FilterResult { get; set; }

        [BindNever]
        public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();

        [BindNever]
        public List<SelectListItem> Categories { get; set; }

        [BindNever]
        public List<SelectListItem> AuditTrailSorts { get; set; }

        // TODO
        [BindNever]
        public List<SelectListItem> AuditTrailDates { get; set; }        

    }

    public enum AuditTrailSort
    {
        Timestamp,
        Category,
        Event,
        User
    }
}
