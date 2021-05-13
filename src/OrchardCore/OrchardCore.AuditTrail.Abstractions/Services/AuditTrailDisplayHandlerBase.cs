using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailDisplayHandlerBase : IAuditTrailDisplayHandler
    {
        public virtual Task DisplayFiltersAsync(DisplayFiltersContext context) => Task.CompletedTask;
        public virtual Task DisplayEventAsync(DisplayEventContext context) => Task.CompletedTask;
    }
}
