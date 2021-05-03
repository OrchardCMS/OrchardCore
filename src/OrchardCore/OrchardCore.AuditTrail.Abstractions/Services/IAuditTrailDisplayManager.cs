using System.Threading.Tasks;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.Services
{
    public interface IAuditTrailDisplayManager
    {
        /// <summary>
        /// Builds a shape tree of filter displays for the providers.
        /// This provides the controls for users to be able to filter between the events.
        /// </summary>
        /// <param name="filters">Input for each filter builder.</param>
        /// <returns>A tree of shapes.</returns>
        Task<IShape> BuildDisplayFiltersAsync(Filters filters);

        /// <summary>
        /// Builds a shape tree of event displays based on the display type.
        /// Use this to construct the shape of the event and add alternates to it.
        /// </summary>
        /// <param name="auditTrailEvent">The AuditTrailEvent.</param>
        /// <param name="displayType">The display type.</param>
        /// <returns>A tree of shapes.</returns>
        Task<IShape> BuildDisplayAsync(AuditTrailEvent auditTrailEvent, string displayType);

        /// <summary>
        /// Builds a shape tree of action displays.
        /// Action displays can be used to provide different types of actions for the given event, like to Restore it.
        /// </summary>
        /// <param name="auditTrailEvent">The AuditTrailEvent.</param>
        /// <param name="displayType">The display type.</param>
        /// <returns>A tree of shapes.</returns>
        Task<IShape> BuildDisplayActionsAsync(AuditTrailEvent auditTrailEvent, string displayType);

        /// <summary>
        /// Builds a shape tree to display the content of custom columns.
        /// </summary>
        /// <param name="auditTrailEvent">The AuditTrailEvent.</param>
        /// <returns>A tree of shapes.</returns>
        Task<IShape> BuildDisplayColumnsAsync(AuditTrailEvent auditTrailEvent);

        /// <summary>
        /// Builds a shape tree to display custom column names.
        /// </summary>
        /// <returns>A tree of shapes.</returns>
        Task<IShape> BuildDisplayColumnNamesAsync();
    }
}
