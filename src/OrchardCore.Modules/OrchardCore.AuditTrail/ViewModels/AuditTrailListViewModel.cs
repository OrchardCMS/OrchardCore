using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels;

public class AuditTrailListViewModel
{
    public IList<IShape> Events { get; set; }
    public AuditTrailIndexOptions Options { get; set; } = new AuditTrailIndexOptions();
    public IShape Pager { get; set; }
    public dynamic Header { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the events, the header and the pager in the configured layout.
    /// </summary>
    public IShape List { get; set; }
}
