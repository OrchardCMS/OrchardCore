using OrchardCore.Admin;
using OrchardCore.AuditTrail.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels;

public class AuditTrailListViewModel
{
    /// <summary>
    /// The queried audit events on this page, before their row shapes are built.
    /// </summary>
    public IList<AuditTrailEvent> Events { get; set; }
    public AuditTrailIndexOptions Options { get; set; } = new AuditTrailIndexOptions();
    public IShape Pager { get; set; }
    public IShape Header { get; set; }

    /// <summary>
    /// The data context for rendering the events, the header and the pager in the configured layout.
    /// </summary>
    public AdminListContext List { get; set; }
}
