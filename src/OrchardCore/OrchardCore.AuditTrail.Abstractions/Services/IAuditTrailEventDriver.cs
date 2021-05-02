using System.Threading.Tasks;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;

namespace OrchardCore.AuditTrail.Services
{
    /// <summary>
    /// Describes an event driver that can change the behavior of Audit Trail rendering.
    /// </summary>
    public interface IAuditTrailEventDriver
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
        /// Adds custom data for rendering an Audit Trail event.
        /// </summary>
        Task BuildViewModelAsync(AuditTrailEventViewModel model);
    }
}
