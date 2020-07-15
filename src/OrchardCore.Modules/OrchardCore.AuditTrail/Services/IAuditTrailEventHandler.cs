using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement.Shapes;
using System.Threading.Tasks;

namespace OrchardCore.AuditTrail.Services
{
    /// <summary>
    /// Described an event handler that can change the behavior of Audit Trail interactions.
    /// </summary>
    public interface IAuditTrailEventHandler
    {
        /// <summary>
        /// Modify the context when creating an Audit Trail event.
        /// </summary>
        Task CreateAsync(AuditTrailCreateContext context);

        /// <summary>
        /// Filter Audit Trail events based on the values from the query string.
        /// </summary>
        void Filter(QueryFilterContext context);

        /// <summary>
        /// Add custom UI elements which can be used to filter Audit Trail events.
        /// </summary>
        Task DisplayFilterAsync(DisplayFilterContext context);

        /// <summary>
        /// Fill the additional columns with data based on the current Audit Trail event.
        /// </summary>
        Task DisplayAdditionalColumnsAsync(DisplayAdditionalColumnsContext context);

        /// <summary>
        /// Add custom column names to the Audit Trail events table.
        /// </summary>
        Task DisplayAdditionalColumnNamesAsync(Shape display);
    }
}
