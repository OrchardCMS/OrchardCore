using System.Threading.Tasks;
using GraphQL;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Modules;

namespace OrchardCore.Contents.AuditTrail.Services
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class ContentAuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        public override Task CreateAsync(AuditTrailCreateContext context)
        {
            if (context.Category != "Content")
            {
                return Task.CompletedTask;
            }

            var content = context.Data.Get<ContentItem>("ContentItem");
            if (content == null)
            {
                return Task.CompletedTask;
            }

            var auditTrailPart = content.ContentItem.As<AuditTrailPart>();
            if (auditTrailPart == null)
            {
                return Task.CompletedTask;
            }

            context.Comment = auditTrailPart.Comment;

            return Task.CompletedTask;
        }
    }
}
