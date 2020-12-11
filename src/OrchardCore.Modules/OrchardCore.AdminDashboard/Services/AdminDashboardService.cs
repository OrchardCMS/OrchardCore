using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OrchardCore.AdminDashboard.Indexes;
using OrchardCore.AdminDashboard.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Documents;
using YesSql;

namespace OrchardCore.AdminDashboard.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ISession _session;
        //private readonly IDocumentManager<LayersDocument> _documentManager;

        public AdminDashboardService(ISession session) //, IDocumentManager<LayersDocument> documentManager)
        {
            _session = session;
            //_documentManager = documentManager;
        }

        public async Task<IEnumerable<ContentItem>> GetWidgetsAsync(Expression<Func<ContentItemIndex, bool>> predicate)
        {
            return await _session
                .Query<ContentItem, DashboardPartIndex>()
                .With(predicate)
                .ListAsync();
        }
    }
}
