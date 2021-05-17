using System.Collections.Generic;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailViewModel
    {
        public IShape FiltersShape { get; set; }
        public AuditTrailOrderBy OrderBy { get; set; }
        public IEnumerable<AuditTrailEventSummaryViewModel> Events { get; set; }
        public IShape PagerShape { get; set; }
    }
}
