using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Contents.AuditTrail.Handlers
{
    public interface IAuditTrailContentHandler : IContentHandler
    {
        Task RestoringAsync(RestoreContentContext context);
        Task RestoredAsync(RestoreContentContext context);
    }
}
