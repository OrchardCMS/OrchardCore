using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using YesSql;
using YesSql.Filters.Abstractions.Nodes;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListQueryService : IContentsAdminListQueryService
    {
        public const string DefaultTermName = "text";

        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IContentsAdminListFilter> _contentsAdminListFilters;
        private readonly ContentSearchOption _contentSearchOptions;
        private readonly ILogger _logger;

        public DefaultContentsAdminListQueryService(
            ISession session,
            IServiceProvider serviceProvider,
            IEnumerable<IContentsAdminListFilter> contentsAdminListFilters,
            IOptions<ContentSearchOption> contentSearchOptions,
            ILogger<DefaultContentsAdminListQueryService> logger)
        {
            _session = session;
            _serviceProvider = serviceProvider;
            _contentsAdminListFilters = contentsAdminListFilters;
            _contentSearchOptions = contentSearchOptions.Value;
            _logger = logger;
        }

        public async Task<IQuery<ContentItem>> QueryAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            var selectedContentType = GetSelectedContentType(model);

            var defaultTermNode = model.FilterResult.OfType<DefaultTermNode>().FirstOrDefault();
            var defaultTermName = defaultTermNode?.TermName;
            var defaultOperator = defaultTermNode?.Operation;

            if (defaultTermNode is not null)
            {
                defaultTermName = GetDefaultTermName(selectedContentType);

                if (defaultTermName != defaultTermNode.TermName || defaultOperator != defaultTermNode.Operation)
                {
                    model.FilterResult.TryRemove(defaultTermNode.TermName);
                    model.FilterResult.TryAddOrReplace(new DefaultTermNode(defaultTermName, defaultOperator));
                }
            }

            // Because admin filters can add a different index to the query this must be added as a Query<ContentItem>().
            var query = _session.Query<ContentItem>();

            query = await model.FilterResult.ExecuteAsync(new ContentQueryContext(_serviceProvider, query));

            // After the q=xx filters have been applied, allow the secondary filter providers to also parse other values for filtering.
            await _contentsAdminListFilters
                .InvokeAsync((filter, model, query, updater) => filter.FilterAsync(model, query, updater), model, query, updater, _logger);

            if (defaultTermName != defaultTermNode?.TermName || defaultOperator != defaultTermNode?.Operation)
            {
                // Restore the original defaultTermNode.
                model.FilterResult.TryRemove(defaultTermName);
                model.FilterResult.TryAddOrReplace(defaultTermNode);
            }

            return query;
        }

        private static string GetSelectedContentType(ContentOptionsViewModel model)
        {
            var selectedContentType = model.SelectedContentType;
            if (selectedContentType == null)
            {
                var typeTermNode = model.FilterResult.OfType<TermOperationNode>()
                    .FirstOrDefault(x => x.TermName == "type" || x.TermName == "stereotype");

                if (typeTermNode != null)
                {
                    selectedContentType = typeTermNode.Operation.ToString();
                }
            }

            return selectedContentType;
        }

        private string GetDefaultTermName(string selectedContentType)
        {
            if (!String.IsNullOrEmpty(selectedContentType) && _contentSearchOptions.TryGetValue(selectedContentType, out var termName))
            {
                return termName;
            }

            return DefaultTermName;
        }
    }
}
