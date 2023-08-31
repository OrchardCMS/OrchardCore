using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using YesSql;
using YesSql.Filters.Abstractions.Nodes;

namespace OrchardCore.Users.Services;

public class DefaultUsersAdminListQueryService : IUsersAdminListQueryService
{
    private readonly static string[] _operators = new[] { "OR", "AND", "||", "&&" };

    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly UsersAdminListFilterOptions _userAdminListFilterOptions;
    private readonly IEnumerable<IUsersAdminListFilter> _usersAdminListFilters;

    public DefaultUsersAdminListQueryService(
        ISession session,
        IServiceProvider serviceProvider,
        ILogger<DefaultUsersAdminListQueryService> logger,
        IOptions<UsersAdminListFilterOptions> userAdminListFilterOptions,
        IEnumerable<IUsersAdminListFilter> usersAdminListFilters)
    {
        _session = session;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _userAdminListFilterOptions = userAdminListFilterOptions.Value;
        _usersAdminListFilters = usersAdminListFilters;
    }

    public async Task<IQuery<User>> QueryAsync(UserIndexOptions options, IUpdateModel updater)
    {
        var defaultTermNode = options.FilterResult.OfType<DefaultTermNode>().FirstOrDefault();
        var defaultOperator = defaultTermNode?.Operation;
        var defaultTermName = String.IsNullOrEmpty(_userAdminListFilterOptions.TermName)
            ? UsersAdminListFilterOptions.DefaultTermName
            : _userAdminListFilterOptions.TermName;

        if (defaultTermNode is not null)
        {
            var value = defaultTermNode.ToString();
            if (_userAdminListFilterOptions.UseExactMatch
                && !_operators.Any(op => value.Contains(op, StringComparison.Ordinal)))
            {
                // Use an unary operator based on a full quoted string.
                defaultOperator = new UnaryNode(value.Trim('"'), OperateNodeQuotes.Double);
            }

            if (defaultTermName != defaultTermNode.TermName || defaultOperator != defaultTermNode.Operation)
            {
                options.FilterResult.TryRemove(defaultTermNode.TermName);
                options.FilterResult.TryAddOrReplace(new DefaultTermNode(defaultTermName, defaultOperator));
            }
        }

        // Because admin filters can add a different index to the query this must be added as a Query<User>().
        var query = _session.Query<User>();

        query = await options.FilterResult.ExecuteAsync(new UserQueryContext(_serviceProvider, query));

        await _usersAdminListFilters.InvokeAsync((filter, model, query, updater) => filter.FilterAsync(model, query, updater), options, query, updater, _logger);

        if (defaultOperator != defaultTermNode?.Operation)
        {
            // Restore the original 'defaultTermNode'.
            options.FilterResult.TryRemove(defaultTermName);
            options.FilterResult.TryAddOrReplace(defaultTermNode);
        }

        return query;
    }
}
