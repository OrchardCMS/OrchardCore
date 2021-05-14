using System.Threading.Tasks;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.Services
{
    public interface IAuditTrailDisplayManager
    {
        /// <summary>
        /// Builds a shape to render all provided event filters.
        /// </summary>
        /// <param name="filters">The <see cref="Filters"/> builder.</param>
        /// <returns>An <see cref="IShape"/>.</returns>
        Task<IShape> BuildDisplayFiltersAsync(Filters filters);

        /// <summary>
        /// Builds a shape to render an <see cref="AuditTrailEvent"/> in a given display type.
        /// </summary>
        /// <param name="auditTrailEvent">The <see cref="AuditTrailEvent"/>.</param>
        /// <param name="displayType">The display type.</param>
        /// <returns>An <see cref="IShape"/>.</returns>
        Task<IShape> BuildDisplayEventAsync(AuditTrailEvent auditTrailEvent, string displayType);

        /// <summary>
        /// Builds a shape to render all provided event actions.
        /// </summary>
        /// <param name="auditTrailEvent">The <see cref="AuditTrailEvent"/>.</param>
        /// <param name="displayType">The display type.</param>
        /// <returns>An <see cref="IShape"/>.</returns>
        Task<IShape> BuildDisplayActionsAsync(AuditTrailEvent auditTrailEvent, string displayType);
    }
}
