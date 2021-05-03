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
        /// Fills custom columns with data based on the current Audit Trail event.
        /// </summary>
        Task DisplayColumnsAsync(DisplayColumnsContext context);

        /// <summary>
        /// Adds custom column names to the Audit Trail events table.
        /// </summary>
        Task DisplayColumnNamesAsync(DisplayColumnNamesContext context);

        /// <summary>
        /// Adds custom data while rendering a given Audit Trail event.
        /// </summary>
        Task DisplayEventAsync(DisplayEventContext context);
    }
}
