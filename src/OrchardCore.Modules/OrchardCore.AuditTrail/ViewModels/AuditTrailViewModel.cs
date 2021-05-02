using System.Collections.Generic;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailViewModel
    {
        public IEnumerable<AuditTrailEventSummaryViewModel> AuditTrailEvents { get; set; }
        public AuditTrailOrderBy OrderBy { get; set; }
        public IShape ColumnNamesShape { get; set; }
        public IShape FiltersShape { get; set; }
        public IShape PagerShape { get; set; }
    }
}
