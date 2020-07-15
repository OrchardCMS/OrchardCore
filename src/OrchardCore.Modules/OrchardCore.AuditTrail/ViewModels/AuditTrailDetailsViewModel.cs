using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailDetailsViewModel
    {
        public AuditTrailEvent AuditTrailEvent { get; set; }
        public AuditTrailEventDescriptor Descriptor { get; set; }
        public IShape DetailsShape { get; set; }
    }
}
