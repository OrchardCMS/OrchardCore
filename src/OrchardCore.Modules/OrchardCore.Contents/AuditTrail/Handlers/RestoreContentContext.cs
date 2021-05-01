using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Contents.AuditTrail.Handlers
{
    public class RestoreContentContext : ContentContextBase
    {
        public RestoreContentContext(ContentItem contentItem) : base(contentItem)
        {
        }
    }
}
