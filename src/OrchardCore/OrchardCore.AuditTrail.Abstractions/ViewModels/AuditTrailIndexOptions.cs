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

        /// <summary>
        /// Event is a supported UI filter if a <see cref="CorrelationId"/> is provided from the route
        /// </summary>
        public string Event { get; set; }
        public string UserName { get; set; }
        public string CorrelationId { get; set; }

        /// <summary>
        /// Marks a <see cref="CorrelationId"/> as provided from the route, rather than a filter
        /// </summary>
        public bool CorrelationIdFromRoute { get; set; }
        public string Date { get; set; }
        public string Sort { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int EventsCount { get; set; }
        public int TotalItemCount { get; set; }

        [ModelBinder(BinderType = typeof(AuditTrailFilterEngineModelBinder), Name = nameof(SearchText))]
        public QueryFilterResult<AuditTrailEvent> FilterResult { get; set; }

        [BindNever]
        public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();

        [BindNever]
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Events for the current category when a <see cref="CorrelationId"/> is set from the route.
        /// </summary>
        [BindNever]
        public List<SelectListItem> Events { get; set; } = new List<SelectListItem>();

        [BindNever]
        public List<SelectListItem> AuditTrailSorts { get; set; } = new List<SelectListItem>();

        [BindNever]
        public List<SelectListItem> AuditTrailDates { get; set; }   = new List<SelectListItem>();
    }
}
