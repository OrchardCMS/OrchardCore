using OrchardCore.AuditTrail.Services.Models;
using System.Threading.Tasks;

namespace OrchardCore.AuditTrail.Services
{
    /// <summary>
    /// Events for when content item Audit Trail events are being recorded.
    /// </summary>
    public interface IAuditTrailContentEventHandler
    {
        /// <summary>
        /// Invoked when a content item event is being built.
        /// </summary>
        Task BuildingAuditTrailEventAsync(BuildingAuditTrailEventContext buildingAuditTrailEventContext);
    }
}
