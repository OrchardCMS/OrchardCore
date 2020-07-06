using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListQueryProvider : IContentsAdminListQueryProvider
    {
        private readonly ISession _session;
        private readonly IEnumerable<IContentsAdminListFilter> _contentsAdminListFilters;
        private readonly ILogger _logger;

        public DefaultContentsAdminListQueryProvider(
            ISession session,
            IEnumerable<IContentsAdminListFilter> contentsAdminListFilters,
            ILogger<DefaultContentsAdminListQueryProvider> logger)
        {
            _session = session;
            _contentsAdminListFilters = contentsAdminListFilters;
            _logger = logger;
        }

        public async Task<IQuery<ContentItem>> ProvideQueryAsync(IUpdateModel updateModel)
        {
            // Because admin filters can add a different index to the query this must be added as a Query<ContentItem>()
            var query = _session.Query<ContentItem>();

            await _contentsAdminListFilters.InvokeAsync((filter, query, updateModel) => filter.FilterAsync(query, updateModel), query, updateModel, _logger);

            return query;
        }
    }
}
