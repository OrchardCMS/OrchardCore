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

    }
}
