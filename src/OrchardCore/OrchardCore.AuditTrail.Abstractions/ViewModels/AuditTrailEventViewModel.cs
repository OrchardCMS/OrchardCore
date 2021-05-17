using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailEventViewModel
    {
        public AuditTrailEvent Event { get; set; }
        public AuditTrailEventDescriptor Descriptor { get; set; }
    }
}
