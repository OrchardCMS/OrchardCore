using System.Threading.Tasks;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;

namespace OrchardCore.Contents.AuditTrail.Services
{
    public class ContentAuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        public override Task CreateAsync(AuditTrailCreateContext context)
        {
            if (context.Category != "Content")
            {
                return Task.CompletedTask;
            }

            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent)
            {
                var auditTrailPart = contentEvent.AuditTrailEventItem.ContentItem.As<AuditTrailPart>();
                if (auditTrailPart == null)
                {
                    return Task.CompletedTask;
                }

                contentEvent.AuditTrailEventItem.Comment = auditTrailPart.Comment;
            }

            return Task.CompletedTask;
        }
    }
}
