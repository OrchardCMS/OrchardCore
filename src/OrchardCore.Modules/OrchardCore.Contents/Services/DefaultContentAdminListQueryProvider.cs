using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentAdminListQueryProvider : IContentAdminListQueryProvider
    {
        private readonly ISession _session;
        private readonly IEnumerable<IContentAdminFilter> _contentAdminFilters;
        private readonly ILogger _logger;

        public DefaultContentAdminListQueryProvider(
            ISession session,
            IEnumerable<IContentAdminFilter> contentAdminFilters,
            ILogger<DefaultContentAdminListQueryProvider> logger)
        {
            _session = session;
            _contentAdminFilters = contentAdminFilters;
            _logger = logger;
        }

        public async Task<IQuery<ContentItem>> ProvideQueryAsync(ListContentsViewModel model, PagerParameters pagerParameters, IUpdateModel updateModel)
        {
            // Because admin filters can add a different index to the query this must be added as a Query<ContentItem>()
            var query = _session.Query<ContentItem>();

            await _contentAdminFilters.InvokeAsync((filter, query, model, pagerParameters, updateModel) => filter.FilterAsync(query, model, pagerParameters, updateModel), query, model, pagerParameters, updateModel, _logger);

            return query;
        }
    }
}
