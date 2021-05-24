using System;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.AuditTrail.ViewModels
{
    public class AuditTrailContentEventDetailViewModel : AuditTrailContentEventViewModel
    {
        public DiffNode[] DiffNodes { get; set; } = Array.Empty<DiffNode>();
        public string Previous { get; set; }
        public string Current { get; set; }
        public ContentItem PreviousContentItem { get; set; }
    }
}
