using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;

namespace OrchardCore.AdminDashboard.Services
{
    /// <summary>
    /// Provides services to manage the Admin Dashboards.
    /// </summary>
    public interface IAdminDashboardService
    {
        Task<IEnumerable<ContentItem>> GetWidgetsAsync(Expression<Func<ContentItemIndex, bool>> predicate);
    }
}
