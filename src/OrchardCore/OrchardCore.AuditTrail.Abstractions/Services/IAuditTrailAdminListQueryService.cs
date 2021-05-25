using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.ViewModels;
using YesSql;

namespace OrchardCore.AuditTrail.Services
{
    public interface IAuditTrailAdminListQueryService
    {
        Task<IQuery<AuditTrailEvent>> QueryAsync(AuditTrailIndexOptions options, IUpdateModel updater);
    }
}
