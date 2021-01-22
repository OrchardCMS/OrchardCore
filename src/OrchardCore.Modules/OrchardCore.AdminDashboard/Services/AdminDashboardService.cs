using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OrchardCore.AdminDashboard.Indexes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.AdminDashboard.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ISession _session;

        public AdminDashboardService(ISession session)
        {
            _session = session;
        }

        public async Task<IEnumerable<ContentItem>> GetWidgetsAsync(Expression<Func<ContentItemIndex, bool>> predicate)
        {
            var widgets = await _session
                .Query<ContentItem, DashboardPartIndex>()
                .OrderBy(w => w.Position)
                .With(predicate)
                .ListAsync();

            return widgets;
        }
    }
}
