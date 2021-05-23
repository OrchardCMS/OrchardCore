using System;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.Contents.AuditTrail.ViewModels
{
    public class AuditTrailContentEventDetailViewModel : AuditTrailContentEventViewModel
    {
        public DiffNode[] DiffNodes { get; set; } = Array.Empty<DiffNode>();
    }
}
