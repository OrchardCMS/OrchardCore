using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailEventHandlerBase : IAuditTrailEventHandler
    {
        public IStringLocalizer T { get; set; }

        public virtual Task CreateAsync(AuditTrailCreateContext context) => Task.CompletedTask;
        public virtual Task AlterAsync(AuditTrailCreateContext context, AuditTrailEvent auditTrailEvent) => Task.CompletedTask;
        public virtual Task FilterAsync(QueryFilterContext context) => Task.CompletedTask;
    }
}
