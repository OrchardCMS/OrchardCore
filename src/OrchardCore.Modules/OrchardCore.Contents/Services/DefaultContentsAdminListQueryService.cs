using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Data;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using YesSql;
using YesSql.Filters.Nodes;

namespace OrchardCore.Contents.Services;

public sealed class DefaultContentsAdminListQueryService : IContentsAdminListQueryService
{
    private static readonly string[] s_operators = ["OR", "AND", "||", "&&"];

    // The terms of DefaultContentsAdminListFilterProvider that only use ContentItemIndex, which has a single record per document.
    private static readonly HashSet<string> s_singleRecordTerms = new(StringComparer.OrdinalIgnoreCase) { "status", "sort", "type", "stereotype" };

    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IContentsAdminListFilter> _contentsAdminListFilters;
    private readonly ContentsAdminListFilterOptions _contentsAdminListFilterOptions;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public DefaultContentsAdminListQueryService(
        ISession session,
        IServiceProvider serviceProvider,
        IEnumerable<IContentsAdminListFilter> contentsAdminListFilters,
        IOptions<ContentsAdminListFilterOptions> contentsAdminListFilterOptions,
        ShellSettings shellSettings,
        ILogger<DefaultContentsAdminListQueryService> logger)
    {
        _session = session;
        _serviceProvider = serviceProvider;
        _contentsAdminListFilters = contentsAdminListFilters;
        _contentsAdminListFilterOptions = contentsAdminListFilterOptions.Value;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public async Task<IQuery<ContentItem>> QueryAsync(ContentOptionsViewModel model, IUpdateModel updater)
    {
        ArgumentNullException.ThrowIfNull(model);

        DefaultTermNode defaultTermNode = null;
        string defaultTermName = null;
        OperatorNode defaultOperator = null;

        var hasFilterResult = model.FilterResult is not null;

        if (hasFilterResult)
        {
            defaultTermNode = model.FilterResult
                .OfType<DefaultTermNode>()
                .FirstOrDefault();

            if (defaultTermNode is not null)
            {
                defaultTermName = defaultTermNode.TermName;
                defaultOperator = defaultTermNode.Operation;

                var value = defaultTermNode.ToString();
                if (_contentsAdminListFilterOptions.UseExactMatch
                    && !s_operators.Any(op => value.Contains(op, StringComparison.Ordinal)))
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
        }

        // Because admin filters can add a different index to the query this must be added as a 'Query<ContentItem>()'.
        var query = _session.Query<ContentItem>();
        var context = new ContentQueryContext(_serviceProvider, query);

        if (hasFilterResult)
        {
            query = await model.FilterResult.ExecuteAsync(context);
        }

        // After the 'q=xx' filters have been applied, allow the secondary filter providers to also parse other values for filtering.
        await _contentsAdminListFilters
            .InvokeAsync((filter, model, query, updater) => filter.FilterAsync(model, query, updater), model, query, updater, _logger);

        if (hasFilterResult &&
            defaultTermNode is not null &&
            (defaultTermName != defaultTermNode?.TermName || defaultOperator != defaultTermNode?.Operation))
        {
            // Restore the original 'defaultTermNode'.
            model.FilterResult.TryRemove(defaultTermName);
            model.FilterResult.TryAddOrReplace(defaultTermNode);
        }

        if (context.OrderedByModified && CanSkipDeduplication(model))
        {
            query = query.NoDuplicates();
        }

        return query;
    }

    // YesSql removes the duplicates an index with several records per document can add by grouping the matches by document before
    // sorting them, so a page reads every match. Without such an index nothing needs to be removed, and a page stops as soon as it's
    // full when an index serves the order, which the indexes on ModifiedUtc do for the modified date.
    //
    // The other orders stay de-duplicated, as no index serves them. A search too, as a text matching nothing would walk the whole
    // index instead of only reading the matches of its other filters. And SQLite, which doesn't choose the index: it would sort every
    // match with the content of its document, where the de-duplicated query only sorts document IDs.
    private bool CanSkipDeduplication(ContentOptionsViewModel model)
        => _shellSettings["DatabaseProvider"] != DatabaseProviderValue.Sqlite
        && model.FilterResult.All(term => s_singleRecordTerms.Contains(term.TermName))
        && model.FilterResult.TermOptions.All(option => !option.Value.AlwaysRun || s_singleRecordTerms.Contains(option.Key))
        && _contentsAdminListFilters.All(filter => !filter.CanReturnDuplicates);

    private static string GetSelectedContentType(ContentOptionsViewModel model)
    {
        if (string.IsNullOrEmpty(model.SelectedContentType))
        {
            var typeTermNode = model.FilterResult.OfType<ContentTypeFilterNode>().FirstOrDefault();
            if (typeTermNode is not null)
            {
                return typeTermNode.Operation.ToString();
            }

            var stereotypeTermNode = model.FilterResult.OfType<StereotypeFilterNode>().FirstOrDefault();
            if (stereotypeTermNode is not null)
            {
                return stereotypeTermNode.Operation.ToString();
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
