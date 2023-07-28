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

namespace OrchardCore.Contents.Services;

public class DefaultContentsAdminListQueryService : IContentsAdminListQueryService
{
    private readonly static string[] _operators = new[] { "OR", "AND", "||", "&&" };

    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IContentsAdminListFilter> _contentsAdminListFilters;
    private readonly ContentsAdminListFilterOptions _contentsAdminListFilterOptions;
    private readonly ILogger _logger;

    public DefaultContentsAdminListQueryService(
        ISession session,
        IServiceProvider serviceProvider,
        IEnumerable<IContentsAdminListFilter> contentsAdminListFilters,
        IOptions<ContentsAdminListFilterOptions> contentsAdminListFilterOptions,
        ILogger<DefaultContentsAdminListQueryService> logger)
    {
        _session = session;
        _serviceProvider = serviceProvider;
        _contentsAdminListFilters = contentsAdminListFilters;
        _contentsAdminListFilterOptions = contentsAdminListFilterOptions.Value;
        _logger = logger;
    }

    public async Task<IQuery<ContentItem>> QueryAsync(ContentOptionsViewModel model, IUpdateModel updater)
    {
        var defaultTermNode = model.FilterResult.OfType<DefaultTermNode>().FirstOrDefault();
        var defaultTermName = defaultTermNode?.TermName;
        var defaultOperator = defaultTermNode?.Operation;

        if (defaultTermNode is not null)
        {
            var value = defaultTermNode.ToString();
            if (_contentsAdminListFilterOptions.UseExactMatch
                && !_operators.Any(op => value.Contains(op, StringComparison.Ordinal)))
            {
                // Use an unary operator based on a full quoted string.
                defaultOperator = new UnaryNode(value.Trim('"'), OperateNodeQuotes.Double);
            }

            var selectedContentType = GetSelectedContentType(model);
            if (selectedContentType is not null)
            {
                defaultTermName = GetDefaultTermName(selectedContentType);
            }

            if (defaultTermName != defaultTermNode.TermName || defaultOperator != defaultTermNode.Operation)
            {
                model.FilterResult.TryRemove(defaultTermNode.TermName);
                model.FilterResult.TryAddOrReplace(new DefaultTermNode(defaultTermName, defaultOperator));
            }
        }

        // Because admin filters can add a different index to the query this must be added as a 'Query<ContentItem>()'.
        var query = _session.Query<ContentItem>();

        query = await model.FilterResult.ExecuteAsync(new ContentQueryContext(_serviceProvider, query));

        // After the 'q=xx' filters have been applied, allow the secondary filter providers to also parse other values for filtering.
        await _contentsAdminListFilters
            .InvokeAsync((filter, model, query, updater) => filter.FilterAsync(model, query, updater), model, query, updater, _logger);

        if (defaultOperator != defaultTermNode?.Operation)
        {
            // Restore the original 'defaultTermNode'.
            model.FilterResult.TryRemove(defaultTermName);
            model.FilterResult.TryAddOrReplace(defaultTermNode);
        }

        return query;
    }

    private static string GetSelectedContentType(ContentOptionsViewModel model)
    {
        if (String.IsNullOrEmpty(model.SelectedContentType))
        {
            var typeTermNode = model.FilterResult.OfType<ContentTypeFilterNode>().FirstOrDefault();
            if (typeTermNode is not null)
            {
                return typeTermNode.Operation.ToString();
            }

            var sterotypeTermNode = model.FilterResult.OfType<StereotypeFilterNode>().FirstOrDefault();
            if (sterotypeTermNode is not null)
            {
                return sterotypeTermNode.Operation.ToString();
            }

            return null;
        }

        return model.SelectedContentType;
    }

    private string GetDefaultTermName(string selectedContentType)
    {
        if (_contentsAdminListFilterOptions.DefaultTermNames.TryGetValue(selectedContentType, out var termName))
        {
            return termName;
        }

        return ContentsAdminListFilterOptions.DefaultTermName;
    }
}
