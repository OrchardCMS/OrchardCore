using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailDisplayViewModel
    {
        public AuditTrailEvent Event { get; set; }
        public AuditTrailEventDescriptor Descriptor { get; set; }
        public IShape EventShape { get; set; }
    }
}
