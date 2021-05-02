using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Services
{
    public interface IAuditTrailContentHandler : IContentHandler
    {
        Task RestoringAsync(RestoreContentContext context);
        Task RestoredAsync(RestoreContentContext context);
    }
}
