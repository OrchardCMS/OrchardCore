using System.Threading.Tasks;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Services
{
    /// <summary>
    /// Describes an event handler that can change the behavior of Audit Trail interactions.
    /// </summary>
    public interface IAuditTrailEventHandler
    {
        /// <summary>
        /// Modifies the context when creating an Audit Trail event.
        /// </summary>
        Task CreateAsync(AuditTrailCreateContext context);

        /// <summary>
        /// Alters Audit Trail event document after it has been built and before saving it.
        /// </summary>
        Task AlterAsync(AuditTrailCreateContext context, AuditTrailEvent auditTrailEvent);

        /// <summary>
        /// Filters Audit Trail events based on the values from the query string.
        /// </summary>
        void Filter(QueryFilterContext context);
    }
}
