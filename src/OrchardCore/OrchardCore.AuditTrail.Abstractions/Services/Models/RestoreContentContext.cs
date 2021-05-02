using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class RestoreContentContext : ContentContextBase
    {
        public RestoreContentContext(ContentItem contentItem) : base(contentItem)
        {
        }
    }
}
