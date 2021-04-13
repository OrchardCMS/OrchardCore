using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListQueryService : IContentsAdminListQueryService
    {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IContentsAdminListFilter> _contentsAdminListFilters;
        private readonly ILogger _logger;

        public DefaultContentsAdminListQueryService(
            IContentManager contentManager,
            IEnumerable<IContentsAdminListFilter> contentsAdminListFilters,
            ILogger<DefaultContentsAdminListQueryService> logger)
        {
            _contentManager = contentManager;
            _contentsAdminListFilters = contentsAdminListFilters;
            _logger = logger;
        }

        public async Task<IQuery<ContentItem>> QueryAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            // Because admin filters can add a different index to the query this must be added as an IQuery<ContentItem>().
            var query = _contentManager.Query();

            await _contentsAdminListFilters.InvokeAsync((filter, model, query, updater) => filter.FilterAsync(model, query, updater), model, query, updater, _logger);

            return query;
        }
    }
}
