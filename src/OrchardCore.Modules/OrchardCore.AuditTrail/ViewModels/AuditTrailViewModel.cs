using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailViewModel
    {
        public IShape FilterDisplay { get; set; }
        public AuditTrailOrderBy OrderBy { get; set; }
        public IShape AdditionalColumnNames { get; set; }
        public IEnumerable<AuditTrailEventSummaryViewModel> AuditTrailEvents { get; set; }
        public dynamic Pager { get; set; }
    }
}
