using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;

namespace OrchardCore.Contents.AuditTrail.ViewModels
{
    public class AuditTrailContentEventViewModel
    {
        public string Name { get; set; }
        public ContentItem ContentItem { get; set; }
        public int VersionNumber { get; set; }
        public string LatestVersionId { get; set; }
        public string Comment { get; set; }

        [BindNever]
        public AuditTrailEventDescriptor Descriptor { get; set; }

        [BindNever]
        public AuditTrailContentEvent ContentEvent { get; set; }

        [BindNever]
        public AuditTrailEvent AuditTrailEvent { get; set; }
    }
}
