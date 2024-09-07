using System.Linq.Expressions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;

namespace OrchardCore.AdminDashboard.Services;

/// <summary>
/// Provides services to manage the Admin Dashboards.
/// </summary>
public interface IAdminDashboardService
{
    Task<IEnumerable<ContentItem>> GetWidgetsAsync(Expression<Func<ContentItemIndex, bool>> predicate);
}
