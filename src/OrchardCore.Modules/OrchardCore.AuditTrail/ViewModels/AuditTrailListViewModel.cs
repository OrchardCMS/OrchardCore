using System.Collections.Generic;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels
{
    // public class AuditTrailListViewModel
    // {
    //     public IShape FiltersShape { get; set; }
    //     public AuditTrailOrderBy OrderBy { get; set; }
    //     public IEnumerable<AuditTrailItemViewModel> Items { get; set; }
    //     public IShape PagerShape { get; set; }
    // }

    public class AuditTrailListViewModel
    {
        public IList<IShape> Events { get; set; }
        // public UserIndexOptions Options { get; set; } = new UserIndexOptions();
        public IShape Pager { get; set; }
        public IShape Header { get; set; }
    }

    // public class AuditTrailEntry
    // {
    //     public IShape Shape { get; set; }
    //     public string EventId { get; set; }
    // }

}
