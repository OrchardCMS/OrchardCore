using System.Threading.Tasks;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Services
{
    /// <summary>
    /// Describes an event handler participating in Audit Trail rendering.
    /// </summary>
    public interface IAuditTrailDisplayHandler
    {
        /// <summary>
        /// Adds custom UI elements which can be used to filter Audit Trail events.
        /// </summary>
        Task DisplayFiltersAsync(DisplayFiltersContext context);

        /// <summary>
        /// Adds custom data while rendering a given Audit Trail event.
        /// </summary>
        Task DisplayEventAsync(DisplayEventContext context);
    }
}
